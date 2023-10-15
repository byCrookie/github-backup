using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Flurl.Http;
using GithubBackup.Core.Github.Clients;
using Microsoft.Extensions.Logging;
using Polly;

namespace GithubBackup.Core.Github.Migrations;

internal sealed partial class MigrationService : IMigrationService
{
    private readonly IFileSystem _fileSystem;
    private readonly IGithubApiClient _githubApiClient;
    private readonly ILogger<MigrationService> _logger;

    public MigrationService(
        IFileSystem fileSystem,
        IGithubApiClient githubApiClient,
        ILogger<MigrationService> logger)
    {
        _fileSystem = fileSystem;
        _githubApiClient = githubApiClient;
        _logger = logger;
    }

    public async Task<Migration> StartMigrationAsync(StartMigrationOptions options, CancellationToken ct)
    {
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
        var response = await _githubApiClient
            .ReceiveJsonPagedAsync<List<MigrationReponse>, MigrationReponse>(
                "/user/migrations",
                100,
                r => r,
                null,
                ct
            );

        return response.Select(m => new Migration(m.Id, m.State, m.CreatedAt)).ToList();
    }

    public async Task<Migration> GetMigrationAsync(long id, CancellationToken ct)
    {
        var response = await _githubApiClient
            .GetAsync($"/user/migrations/{id}", ct: ct)
            .ReceiveJson<MigrationReponse>();

        return new Migration(response.Id, response.State, response.CreatedAt);
    }

    public async Task<string> PollAndDownloadMigrationAsync(DownloadMigrationOptions options,
        Func<Migration, Task> onPollAsync, CancellationToken ct)
    {
        await Policy
            .HandleResult<Migration>(e => e.State != MigrationState.Exported)
            .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            .ExecuteAsync(async () =>
            {
                var migrationStatus = await GetMigrationAsync(options.Id, ct);
                _logger.LogDebug("Migration {Id} is {State}", migrationStatus.Id, migrationStatus.State);
                await onPollAsync(migrationStatus);
                return migrationStatus;
            });

        return await DownloadMigrationAsync(options, ct);
    }

    public Task<string> DownloadMigrationAsync(DownloadMigrationOptions options, CancellationToken ct)
    {
        if (options.Overwrite)
        {
            OverwriteBackups(options);
        }

        if (options.NumberOfBackups is not null)
        {
            ApplyRetentionRules(options);
        }

        var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_migration_{options.Id}.tar.gz";

        if (_fileSystem.File.Exists(_fileSystem.Path.Combine(options.Destination.FullName, fileName)))
        {
            throw new Exception($"A backup with the id {options.Id} already exists.");
        }

        return _githubApiClient
            .DownloadFileAsync($"/user/migrations/{options.Id}/archive", options.Destination.FullName, fileName, ct: ct);
    }

    private void ApplyRetentionRules(DownloadMigrationOptions options)
    {
        if (options.NumberOfBackups == 0)
        {
            throw new Exception("The number of backups cannot be 0.");
        }

        var backups = _fileSystem.Directory
            .GetFiles(options.Destination.FullName, "*", SearchOption.TopDirectoryOnly)
            .Select(file => BackupFileNameRegex().Match(file))
            .Where(match => match.Success)
            .ToList();

        if (backups.Count < options.NumberOfBackups)
        {
            var backupsToDelete = backups
                .OrderByDescending(match => match.Groups["Date"].Value)
                .Skip(options.NumberOfBackups.Value - 1);

            foreach (var backup in backupsToDelete)
            {
                _logger.LogDebug("Deleting backup {Backup} because to many backups are present", backup.Value);
                _fileSystem.File.Delete(_fileSystem.Path.Combine(options.Destination.FullName, backup.Value));
            }
        }
    }

    private void OverwriteBackups(DownloadMigrationOptions options)
    {
        var identicalBackups = _fileSystem.Directory
            .GetFiles(options.Destination.FullName, "*", SearchOption.TopDirectoryOnly)
            .Select(file => BackupFileNameRegex().Match(file))
            .Where(match => match.Success && match.Groups["Id"].Value == options.Id.ToString())
            .ToList();

        if (!identicalBackups.Any())
        {
            return;
        }

        foreach (var backup in identicalBackups)
        {
            _logger.LogDebug("Deleting identical backup {Backup}", backup.Value);
            _fileSystem.File.Delete(_fileSystem.Path.Combine(options.Destination.FullName, backup.Value));
        }
    }

    [GeneratedRegex(@"^(?<Date>\d{14})_migration_(?<Id>\d+)\.tar\.gz$", RegexOptions.Compiled)]
    private static partial Regex BackupFileNameRegex();
}