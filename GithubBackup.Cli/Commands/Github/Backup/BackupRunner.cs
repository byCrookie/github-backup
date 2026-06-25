using System.IO.Abstractions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Output;
using GithubBackup.Core.Github.Migrations;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal sealed class BackupRunner(
    GlobalArgs globalArgs,
    BackupArgs backupArgs,
    IMigrationService migrationService,
    ILoginService loginService,
    IFileSystem fileSystem,
    ICliOutput output,
    IAnsiConsole ansiConsole
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

        output.Status("Creating migration...");
        var migration = await migrationService.StartMigrationAsync(options, ct);

        output.Status(
            $"Waiting for migration {migration.Id}, then downloading to {backupArgs.DownloadArgs.Destination}..."
        );

        var downloadOptions = new DownloadMigrationOptions(
            migration.Id,
            fileSystem.DirectoryInfo.Wrap(backupArgs.DownloadArgs.Destination),
            backupArgs.DownloadArgs.NumberOfBackups,
            backupArgs.DownloadArgs.Overwrite,
            onTemporaryFileCreated: tempFile => output.Status($"Using temporary file {tempFile}")
        );

        var file = globalArgs.Quiet
            ? await DownloadAsync(downloadOptions, null, ct)
            : await DownloadProgress.RunAsync(
                ansiConsole,
                $"Migration {migration.Id}",
                onProgress => DownloadAsync(downloadOptions, onProgress, ct)
            );

        output.Status($"Downloaded migration {migration.Id} to {file}");
        output.Data(file);
    }

    private Task<string> DownloadAsync(
        DownloadMigrationOptions options,
        Action<long, long?>? onDownloadProgress,
        CancellationToken ct
    )
    {
        options = new DownloadMigrationOptions(
            options.Id,
            options.Destination,
            options.NumberOfBackups,
            options.Overwrite,
            options.MedianFirstRetryDelay,
            options.OnTemporaryFileCreated,
            onDownloadProgress
        );

        return migrationService.PollAndDownloadMigrationAsync(
            options,
            update =>
            {
                output.Status($"Migration {update.Id} is {update.State}...");

                return Task.CompletedTask;
            },
            ct
        );
    }
}
