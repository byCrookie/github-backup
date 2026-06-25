using System.IO.Abstractions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
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
    IAnsiConsole ansiConsole
) : ICommandRunner
{
    public async Task RunAsync(CancellationToken ct)
    {
        await loginService.LoginAsync(globalArgs, downloadArgs.LoginArgs, ct);

        if (downloadArgs.Migrations.Length != 0)
        {
            if (!globalArgs.Quiet)
            {
                ansiConsole.WriteLine("Downloading migrations...");
            }

            logger.LogInformation("Downloading migrations using ids");
            await DownloadUsingIdsAsync(ct);
            return;
        }

        if (downloadArgs.Latest)
        {
            if (!globalArgs.Quiet)
            {
                ansiConsole.WriteLine("Downloading latest migration");
            }

            logger.LogInformation("Downloading latest migration");
            await DownloadLatestAsync(ct);
            return;
        }

        if (!globalArgs.Quiet)
        {
            ansiConsole.WriteLine("No migration ids specified, downloading latest migration");
        }

        logger.LogInformation("No migration ids specified, downloading latest migration");
        await DownloadLatestAsync(ct);
    }

    private async Task DownloadLatestAsync(CancellationToken ct)
    {
        var migrations = await migrationService.GetMigrationsAsync(ct);

        if (migrations.All(e => e.State != MigrationState.Exported))
        {
            logger.LogInformation("No exported migrations found");

            if (!globalArgs.Quiet)
            {
                ansiConsole.WriteLine("No exported migrations found");
            }

            return;
        }

        var migration = migrations
            .OrderBy(m => m.CreatedAt)
            .Last(e => e.State == MigrationState.Exported);

        await DownloadMigrationUsingIdAsync(migration.Id, ct);
    }

    private async Task DownloadUsingIdsAsync(CancellationToken ct)
    {
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

        if (!globalArgs.Quiet)
        {
            ansiConsole.WriteLine($"Downloading migration {id} to {downloadArgs.Destination}...");
            var progress = ansiConsole.Progress();
            progress.RefreshRate = TimeSpan.FromSeconds(5);
            await progress.StartAsync(_ => DownloadMigrationAsync(id, ct));
            return;
        }

        await DownloadMigrationAsync(id, ct);
    }

    private async Task DownloadMigrationAsync(long id, CancellationToken ct)
    {
        var options = new DownloadMigrationOptions(
            id,
            fileSystem.DirectoryInfo.Wrap(downloadArgs.Destination),
            downloadArgs.NumberOfBackups,
            downloadArgs.Overwrite
        );

        var path = await DownloadAsync(options, ct);
        logger.LogInformation("Downloaded migration {Id} to {Path}", id, path);
        ansiConsole.WriteLine(!globalArgs.Quiet ? $"Downloaded migration {id} to {path}" : path);
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
                    if (!globalArgs.Quiet)
                    {
                        ansiConsole.WriteLine($"Migration {update.Id} is {update.State}...");
                    }

                    return Task.CompletedTask;
                },
                ct
            );
        }

        logger.LogInformation("Downloading migration {Id}", options.Id);
        return migrationService.DownloadMigrationAsync(options, ct);
    }
}
