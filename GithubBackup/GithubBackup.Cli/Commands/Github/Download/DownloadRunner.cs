using System.IO.Abstractions;
using GithubBackup.Cli.Commands.Github.Credentials;
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
        var user = await _loginService.ValidateLoginAsync(ct);

        if (!_globalArgs.Quiet)
        {
            _ansiConsole.WriteLine($"Logged in as {user.Name}");
        }

        if (_downloadArgs.Latest)
        {
            _logger.LogInformation("Downloading latest migration");
            await DownloadLatestAsync(ct);
            return;
        }

        if (!_downloadArgs.Migrations.Any())
        {
            _logger.LogInformation("No migration ids specified, downloading latest migration");
            await DownloadLatestAsync(ct);
            return;
        }

        _logger.LogInformation("Downloading migrations using ids");
        await DownloadUsingIdsAsync(ct);
    }

    private async Task DownloadLatestAsync(CancellationToken ct)
    {
        var migrations = await _migrationService.GetMigrationsAsync(ct);

        if (migrations.All(e => e.State != MigrationState.Exported))
        {
            _logger.LogInformation("No migrations found");

            if (!_globalArgs.Quiet)
            {
                _ansiConsole.WriteLine("No migrations found");
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
            await progress.StartAsync(async _ =>
            {
                var path = await DownloadMigrationAsync(id, ct);
                _ansiConsole.WriteLine($"Downloaded migration {id} to {path}");
            });
            return;
        }

        await DownloadMigrationAsync(id, ct);
    }

    private async Task<string> DownloadMigrationAsync(long id, CancellationToken ct)
    {
        var options = new DownloadMigrationOptions(
            id,
            _fileSystem.DirectoryInfo.Wrap(_downloadArgs.Destination),
            _downloadArgs.NumberOfBackups,
            _downloadArgs.Overwrite
        );

        var path = await _migrationService.DownloadMigrationAsync(options, ct);
        _logger.LogInformation("Downloaded migration {Id} to {Path}", id, path);
        return path;
    }
}