using System.IO.Abstractions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Output;
using GithubBackup.Core.Github.Migrations;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class DownloadRunner(
    GlobalArgs globalArgs,
    DownloadArgs downloadArgs,
    IMigrationService migrationService,
    ILoginService loginService,
    IFileSystem fileSystem,
    ILogger<DownloadRunner> logger,
    ICliOutput output,
    IAnsiConsole ansiConsole
) : ICommandRunner
{
    public async Task RunAsync(CancellationToken ct)
    {
        await loginService.LoginAsync(globalArgs, downloadArgs.LoginArgs, ct);

        if (downloadArgs.Migrations.Length != 0)
        {
            output.Status($"Downloading {downloadArgs.Migrations.Length} selected migration(s)...");

            logger.LogInformation("Downloading migrations by ID");
            await DownloadUsingIdsAsync(ct);
            return;
        }

        if (downloadArgs.Latest)
        {
            output.Status("Downloading latest migration");

            logger.LogInformation("Downloading latest migration");
            await DownloadLatestAsync(ct);
            return;
        }

        output.Status("No migration IDs specified; downloading the latest migration.");

        logger.LogInformation("No migration IDs specified; downloading the latest migration");
        await DownloadLatestAsync(ct);
    }

    private async Task DownloadLatestAsync(CancellationToken ct)
    {
        output.Status("Fetching migrations...");
        var migrations = await migrationService.GetMigrationsAsync(ct);

        if (migrations.All(e => e.State != MigrationState.Exported))
        {
            logger.LogInformation("No downloadable migrations found");

            output.Status("No downloadable migrations found.");

            return;
        }

        var migration = migrations
            .OrderBy(m => m.CreatedAt)
            .Last(e => e.State == MigrationState.Exported);

        await DownloadMigrationUsingIdAsync(migration.Id, ct);
    }

    private async Task DownloadUsingIdsAsync(CancellationToken ct)
    {
        output.Status("Fetching migrations...");
        var migrations = (await migrationService.GetMigrationsAsync(ct))
            .OrderBy(m => m.CreatedAt)
            .ToList();

        foreach (
            var id in downloadArgs.Migrations.OrderBy(m => migrations.FindIndex(e => e.Id == m))
        )
        {
            await DownloadMigrationUsingIdAsync(id, ct);
        }
    }

    private async Task DownloadMigrationUsingIdAsync(long id, CancellationToken ct)
    {
        logger.LogInformation(
            "Downloading migration {Id} to {Destination}",
            id,
            downloadArgs.Destination
        );

        string path;

        if (!globalArgs.Quiet)
        {
            output.Status($"Downloading migration {id} to {downloadArgs.Destination}...");
            path = await DownloadProgress.RunAsync(
                ansiConsole,
                $"Migration {id}",
                onProgress => DownloadMigrationAsync(id, onProgress, ct)
            );
        }
        else
        {
            path = await DownloadMigrationAsync(id, null, ct);
        }

        logger.LogInformation("Downloaded migration {Id} to {Path}", id, path);
        output.Status($"Downloaded migration {id} to {path}");
        output.Data(path);
    }

    private async Task<string> DownloadMigrationAsync(
        long id,
        Action<long, long?>? onDownloadProgress,
        CancellationToken ct
    )
    {
        var options = new DownloadMigrationOptions(
            id,
            fileSystem.DirectoryInfo.Wrap(downloadArgs.Destination),
            downloadArgs.NumberOfBackups,
            downloadArgs.Overwrite,
            onTemporaryFileCreated: tempFile =>
                output.Status($"Using temporary file {tempFile}"),
            onDownloadProgress: onDownloadProgress
        );

        var path = await DownloadAsync(options, ct);
        return path;
    }

    private Task<string> DownloadAsync(DownloadMigrationOptions options, CancellationToken ct)
    {
        if (downloadArgs.Poll)
        {
            logger.LogInformation("Polling migration {Id}", options.Id);
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

        logger.LogInformation("Downloading migration {Id}", options.Id);
        return migrationService.DownloadMigrationAsync(options, ct);
    }
}
