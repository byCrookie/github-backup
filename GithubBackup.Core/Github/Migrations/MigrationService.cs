using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Flurl.Http;
using GithubBackup.Core.Github.Clients;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace GithubBackup.Core.Github.Migrations;

internal sealed partial class MigrationService(
    IFileSystem fileSystem,
    IGithubApiClient githubApiClient,
    IDateTimeProvider dateTimeProvider,
    ILogger<MigrationService> logger
) : IMigrationService
{
    public async Task<Migration> StartMigrationAsync(
        StartMigrationOptions options,
        CancellationToken ct
    )
    {
        logger.LogDebug("Creating migration");

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

        var response = await githubApiClient
            .PostJsonAsync("/user/migrations", request, ct: ct)
            .ReceiveJson<MigrationReponse>();

        return new Migration(response.Id, response.State, response.CreatedAt);
    }

    public async Task<List<Migration>> GetMigrationsAsync(CancellationToken ct)
    {
        logger.LogDebug("Fetching migrations");

        var response = await githubApiClient.ReceiveJsonPagedAsync<
            List<MigrationReponse>,
            MigrationReponse
        >("/user/migrations", 100, r => r, null, ct);

        return response.Select(m => new Migration(m.Id, m.State, m.CreatedAt)).ToList();
    }

    public async Task<Migration> GetMigrationAsync(long id, CancellationToken ct)
    {
        logger.LogDebug("Fetching migration {Id}", id);

        var response = await githubApiClient
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
        logger.LogDebug("Polling migration {Id}", options.Id);

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
                logger.LogInformation(
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

        if (migration.CreatedAt < dateTimeProvider.Now.AddDays(-7))
        {
            throw new Exception(
                "The migration is older than 7 days and can no longer be downloaded."
            );
        }

        var fileName = $"{dateTimeProvider.Now:yyyyMMddHHmmss}_migration_{options.Id}.tar.gz";
        var tempFile = fileSystem.Path.Combine(
            fileSystem.Path.GetTempPath(),
            fileSystem.Path.GetRandomFileName()
        );
        var tempDirectoryName = fileSystem.Path.GetDirectoryName(tempFile)!;
        var tempFileName = fileSystem.Path.GetFileName(tempFile);

        logger.LogInformation("Downloading migration {Id} to temporary file {TempFile}", options.Id, tempFile);

        await githubApiClient.DownloadFileAsync(
            $"/user/migrations/{options.Id}/archive",
            tempDirectoryName,
            tempFileName,
            ct: ct
        );

        if (options.Overwrite)
        {
            logger.LogInformation("Overwriting backups");
            OverwriteBackups(options);
        }

        if (options.NumberOfBackups is not null)
        {
            logger.LogInformation("Applying retention rules");
            ApplyRetentionRules(options);
        }

        var migrationPath = fileSystem.Path.Combine(options.Destination.FullName, fileName);
        if (fileSystem.File.Exists(migrationPath))
        {
            throw new Exception($"A backup with ID {options.Id} already exists.");
        }

        fileSystem.File.Move(tempFile, migrationPath);
        return migrationPath;
    }

    private void ApplyRetentionRules(DownloadMigrationOptions options)
    {
        if (options.NumberOfBackups == 0)
        {
            throw new Exception("The number of backups cannot be 0.");
        }

        var backups = fileSystem
            .Directory.GetFiles(options.Destination.FullName, "*", SearchOption.TopDirectoryOnly)
            .Select(file => BackupFileNameRegex().Match(fileSystem.Path.GetFileName(file)))
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
                logger.LogInformation(
                    "Deleting backup {Backup} because the retention limit was exceeded",
                    backup.Value
                );
                fileSystem.File.Delete(
                    fileSystem.Path.Combine(options.Destination.FullName, backup.Value)
                );
            }
        }
        else
        {
            logger.LogInformation("Backup retention limit not exceeded");
        }
    }

    private void OverwriteBackups(DownloadMigrationOptions options)
    {
        var identicalBackups = fileSystem
            .Directory.GetFiles(options.Destination.FullName, "*", SearchOption.TopDirectoryOnly)
            .Select(file => BackupFileNameRegex().Match(fileSystem.Path.GetFileName(file)))
            .Where(match => match.Success && match.Groups["Id"].Value == options.Id.ToString())
            .ToList();

        if (identicalBackups.Count == 0)
        {
            logger.LogInformation("No identical backups found");
            return;
        }

        foreach (var backup in identicalBackups)
        {
            logger.LogInformation("Deleting identical backup {Backup}", backup.Value);
            fileSystem.File.Delete(
                fileSystem.Path.Combine(options.Destination.FullName, backup.Value)
            );
        }
    }

    [GeneratedRegex(@"^(?<Date>\d{14})_migration_(?<Id>\d+)\.tar\.gz$", RegexOptions.Compiled)]
    private static partial Regex BackupFileNameRegex();
}
