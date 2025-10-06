using System.IO.Abstractions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Migrations;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal sealed class BackupRunner(
    GlobalArgs globalArgs,
    BackupArgs backupArgs,
    IMigrationService migrationService,
    ILoginService loginService,
    IFileSystem fileSystem,
    IAnsiConsole ansiConsole)
    : ICommandRunner
{
    public async Task RunAsync(CancellationToken ct)
    {
        await loginService.WithPersistentAsync(globalArgs, backupArgs.LoginArgs, false, ct);

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

        if (!globalArgs.Quiet)
        {
            ansiConsole.WriteLine(
                $"Downloading migration {migration.Id} to {backupArgs.DownloadArgs.Destination} when ready..."
            );
        }

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
                if (!globalArgs.Quiet)
                {
                    ansiConsole.WriteLine($"Migration {update.Id} is {update.State}...");
                }

                return Task.CompletedTask;
            },
            ct
        );

        ansiConsole.WriteLine(
            !globalArgs.Quiet ? $"Downloaded migration {migration.Id} ({file})" : file
        );
    }
}
