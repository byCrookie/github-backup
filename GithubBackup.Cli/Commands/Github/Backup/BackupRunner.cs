using System.IO.Abstractions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Output;
using GithubBackup.Core.Github.Migrations;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal sealed class BackupRunner(
    GlobalArgs globalArgs,
    BackupArgs backupArgs,
    IMigrationService migrationService,
    ILoginService loginService,
    IFileSystem fileSystem,
    ICliOutput output
) : ICommandRunner
{
    public async Task RunAsync(CancellationToken ct)
    {
        await loginService.LoginAsync(globalArgs, backupArgs.LoginArgs, ct);

        var options = new StartMigrationOptions(
            backupArgs.MigrateArgs.Repositories,
            backupArgs.MigrateArgs.LockRepositories,
            backupArgs.MigrateArgs.ExcludeMetadata,
            backupArgs.MigrateArgs.ExcludeGitData,
            backupArgs.MigrateArgs.ExcludeAttachements,
            backupArgs.MigrateArgs.ExcludeReleases,
            backupArgs.MigrateArgs.ExcludeOwnerProjects,
            backupArgs.MigrateArgs.OrgMetadataOnly
        );

        var migration = await migrationService.StartMigrationAsync(options, ct);

        output.Status(
            $"Waiting for migration {migration.Id}, then downloading to {backupArgs.DownloadArgs.Destination}..."
        );

        var downloadOptions = new DownloadMigrationOptions(
            migration.Id,
            fileSystem.DirectoryInfo.Wrap(backupArgs.DownloadArgs.Destination),
            backupArgs.DownloadArgs.NumberOfBackups,
            backupArgs.DownloadArgs.Overwrite
        );

        var file = await migrationService.PollAndDownloadMigrationAsync(
            downloadOptions,
            update =>
            {
                output.Status($"Migration {update.Id} is {update.State}...");

                return Task.CompletedTask;
            },
            ct
        );

        output.Status($"Downloaded migration {migration.Id} to {file}");
        output.Data(file);
    }
}
