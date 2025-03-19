using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Flurl.Http;
using GithubBackup.Core.Github.Clients;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace GithubBackup.Core.Github.Migrations;

internal sealed partial class MigrationService : IMigrationService
{
    private readonly IFileSystem _fileSystem;
    private readonly IGithubApiClient _githubApiClient;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<MigrationService> _logger;

    public MigrationService(
        IFileSystem fileSystem,
        IGithubApiClient githubApiClient,
        IDateTimeProvider dateTimeProvider,
        ILogger<MigrationService> logger
    )
    {
        _fileSystem = fileSystem;
        _githubApiClient = githubApiClient;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Migration> StartMigrationAsync(
        StartMigrationOptions options,
        CancellationToken ct
    )
    {
        _logger.LogDebug("Starting migration");

        var request = new MigrationRequest(
            options.Repositories,
            options.LockRepositories,
            options.ExcludeMetadata,
            options.ExcludeGitData,
            options.ExcludeAttachements,
            options.ExcludeReleases,
            options.ExcludeOwnerProjects,
            options.ExcludeMetadataOnly
        );

        var response = await _githubApiClient
            .PostJsonAsync("/user/migrations", request, ct: ct)
            .ReceiveJson<MigrationReponse>();

        return new Migration(response.Id, response.State, response.CreatedAt);
    }

    public async Task<List<Migration>> GetMigrationsAsync(CancellationToken ct)
    {
        _logger.LogDebug("Getting migrations");

        var response = await _githubApiClient.ReceiveJsonPagedAsync<
            List<MigrationReponse>,
            MigrationReponse
        >("/user/migrations", 100, r => r, null, ct);

        return response.Select(m => new Migration(m.Id, m.State, m.CreatedAt)).ToList();
    }

    public async Task<Migration> GetMigrationAsync(long id, CancellationToken ct)
    {
        _logger.LogDebug("Getting migration {Id}", id);

        var response = await _githubApiClient
            .GetAsync($"/user/migrations/{id}", ct: ct)
            .ReceiveJson<MigrationReponse>();

        return new Migration(response.Id, response.State, response.CreatedAt);
    }

    public async Task<string> PollAndDownloadMigrationAsync(
        DownloadMigrationOptions options,
        Func<Migration, Task> onPollAsync,
        CancellationToken ct
    )
    {
        _logger.LogDebug("Polling migration {Id}", options.Id);

        var resiliencePipeline = new ResiliencePipelineBuilder<Migration>()
            .AddRetry(
                new RetryStrategyOptions<Migration>
                {
                    ShouldHandle = new PredicateBuilder<Migration>().HandleResult(migration =>
                        migration.State != MigrationState.Exported
                    ),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    MaxDelay = TimeSpan.FromHours(1),
                    Delay = options.MedianFirstRetryDelay,
                }
            )
            .Build();

        await resiliencePipeline.ExecuteAsync(
            async cancellationToken =>
            {
                var migrationStatus = await GetMigrationAsync(options.Id, cancellationToken);
                _logger.LogInformation(
                    "Migration {Id} is {State}",
                    migrationStatus.Id,
                    migrationStatus.State
                );
                await onPollAsync(migrationStatus);
                return migrationStatus;
            },
            ct
        );

        return await DownloadMigrationAsync(options, ct);
    }

    public async Task<string> DownloadMigrationAsync(
        DownloadMigrationOptions options,
        CancellationToken ct
    )
    {
        var migration = await GetMigrationAsync(options.Id, ct);

        if (migration.State != MigrationState.Exported)
        {
            throw new Exception(
                $"The migration is not {MigrationState.Exported.GetEnumMemberValue()} and cannot be downloaded."
            );
        }

        if (migration.CreatedAt < _dateTimeProvider.Now.AddDays(-7))
        {
            throw new Exception(
                "The migration is older than 7 days and cannot be downloaded anymore."
            );
        }

        var fileName = $"{_dateTimeProvider.Now:yyyyMMddHHmmss}_migration_{options.Id}.tar.gz";
        var tempFile = _fileSystem.Path.Combine(
            _fileSystem.Path.GetTempPath(),
            _fileSystem.Path.GetRandomFileName()
        );
        var tempDirectoryName = _fileSystem.Path.GetDirectoryName(tempFile)!;
        var tempFileName = _fileSystem.Path.GetFileName(tempFile);

        await _githubApiClient.DownloadFileAsync(
            $"/user/migrations/{options.Id}/archive",
            tempDirectoryName,
            tempFileName,
            ct: ct
        );

        if (options.Overwrite)
        {
            _logger.LogInformation("Overwriting backups");
            OverwriteBackups(options);
        }

        if (options.NumberOfBackups is not null)
        {
            _logger.LogInformation("Applying retention rules");
            ApplyRetentionRules(options);
        }

        var migrationPath = _fileSystem.Path.Combine(options.Destination.FullName, fileName);
        if (_fileSystem.File.Exists(migrationPath))
        {
            throw new Exception($"A backup with the id {options.Id} already exists.");
        }

        _fileSystem.File.Move(tempFile, migrationPath);
        return migrationPath;
    }

    private void ApplyRetentionRules(DownloadMigrationOptions options)
    {
        if (options.NumberOfBackups == 0)
        {
            throw new Exception("The number of backups cannot be 0.");
        }

        var backups = _fileSystem
            .Directory.GetFiles(options.Destination.FullName, "*", SearchOption.TopDirectoryOnly)
            .Select(file => BackupFileNameRegex().Match(_fileSystem.Path.GetFileName(file)))
            .Where(match => match.Success)
            .ToList();

        if (backups.Count > options.NumberOfBackups)
        {
            var backupsToDelete = backups
                .OrderByDescending(match => match.Groups["Date"].Value)
                .Skip(options.NumberOfBackups.Value - 1)
                .ToList();

            foreach (var backup in backupsToDelete)
            {
                _logger.LogInformation(
                    "Deleting backup {Backup} because to many backups are present",
                    backup.Value
                );
                _fileSystem.File.Delete(
                    _fileSystem.Path.Combine(options.Destination.FullName, backup.Value)
                );
            }
        }
        else
        {
            _logger.LogInformation("Not to many backups");
        }
    }

    private void OverwriteBackups(DownloadMigrationOptions options)
    {
        var identicalBackups = _fileSystem
            .Directory.GetFiles(options.Destination.FullName, "*", SearchOption.TopDirectoryOnly)
            .Select(file => BackupFileNameRegex().Match(_fileSystem.Path.GetFileName(file)))
            .Where(match => match.Success && match.Groups["Id"].Value == options.Id.ToString())
            .ToList();

        if (!identicalBackups.Any())
        {
            _logger.LogInformation("No identical backups found");
            return;
        }

        foreach (var backup in identicalBackups)
        {
            _logger.LogInformation("Deleting identical backup {Backup}", backup.Value);
            _fileSystem.File.Delete(
                _fileSystem.Path.Combine(options.Destination.FullName, backup.Value)
            );
        }
    }

    [GeneratedRegex(@"^(?<Date>\d{14})_migration_(?<Id>\d+)\.tar\.gz$", RegexOptions.Compiled)]
    private static partial Regex BackupFileNameRegex();
}
