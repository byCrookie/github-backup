using System.IO.Abstractions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Migrations;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class DownloadRunner : IDownloadRunner
{
    private readonly GlobalArgs _globalArgs;
    private readonly DownloadArgs _downloadArgs;
    private readonly IMigrationService _migrationService;
    private readonly ILoginService _loginService;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DownloadRunner> _logger;
    private readonly IAnsiConsole _ansiConsole;

    public DownloadRunner(
        GlobalArgs globalArgs,
        DownloadArgs downloadArgs,
        IMigrationService migrationService,
        ILoginService loginService,
        IFileSystem fileSystem,
        ILogger<DownloadRunner> logger,
        IAnsiConsole ansiConsole)
    {
        _globalArgs = globalArgs;
        _downloadArgs = downloadArgs;
        _migrationService = migrationService;
        _loginService = loginService;
        _fileSystem = fileSystem;
        _logger = logger;
        _ansiConsole = ansiConsole;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        await _loginService.WithPersistentAsync(
            _globalArgs,
            _downloadArgs.LoginArgs,
            false,
            ct
        );

        if (_downloadArgs.Migrations.Any())
        {
            if (!_globalArgs.Quiet)
            {
                _ansiConsole.WriteLine("Downloading migrations...");
            }

            _logger.LogInformation("Downloading migrations using ids");
            await DownloadUsingIdsAsync(ct);
            return;
        }

        if (_downloadArgs.Latest)
        {
            if (!_globalArgs.Quiet)
            {
                _ansiConsole.WriteLine("Downloading latest migration");
            }

            _logger.LogInformation("Downloading latest migration");
            await DownloadLatestAsync(ct);
            return;
        }

        if (!_globalArgs.Quiet)
        {
            _ansiConsole.WriteLine("No migration ids specified, downloading latest migration");
        }

        _logger.LogInformation("No migration ids specified, downloading latest migration");
        await DownloadLatestAsync(ct);
    }

    private async Task DownloadLatestAsync(CancellationToken ct)
    {
        var migrations = await _migrationService.GetMigrationsAsync(ct);

        if (migrations.All(e => e.State != MigrationState.Exported))
        {
            _logger.LogInformation("No exported migrations found");

            if (!_globalArgs.Quiet)
            {
                _ansiConsole.WriteLine("No exported migrations found");
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
        foreach (var id in _downloadArgs.Migrations)
        {
            await DownloadMigrationUsingIdAsync(id, ct);
        }
    }

    private async Task DownloadMigrationUsingIdAsync(long id, CancellationToken ct)
    {
        _logger.LogInformation("Downloading migration {Id} to {Destination}", id, _downloadArgs.Destination);

        if (!_globalArgs.Quiet)
        {
            _ansiConsole.WriteLine($"Downloading migration {id} to {_downloadArgs.Destination}...");
            var progress = _ansiConsole.Progress();
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
            _fileSystem.DirectoryInfo.Wrap(_downloadArgs.Destination),
            _downloadArgs.NumberOfBackups,
            _downloadArgs.Overwrite
        );

        var path = await _migrationService.DownloadMigrationAsync(options, ct);
        _logger.LogInformation("Downloaded migration {Id} to {Path}", id, path);
        _ansiConsole.WriteLine(!_globalArgs.Quiet ? $"Downloaded migration {id} to {path}" : path);
    }
}