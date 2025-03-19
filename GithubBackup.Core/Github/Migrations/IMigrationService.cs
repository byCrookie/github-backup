namespace GithubBackup.Core.Github.Migrations;

public interface IMigrationService
{
    Task<Migration> StartMigrationAsync(StartMigrationOptions options, CancellationToken ct);
    Task<List<Migration>> GetMigrationsAsync(CancellationToken ct);
    Task<Migration> GetMigrationAsync(long id, CancellationToken ct);
    Task<string> PollAndDownloadMigrationAsync(
        DownloadMigrationOptions options,
        Func<Migration, Task> onPollAsync,
        CancellationToken ct
    );
    Task<string> DownloadMigrationAsync(DownloadMigrationOptions options, CancellationToken ct);
}
