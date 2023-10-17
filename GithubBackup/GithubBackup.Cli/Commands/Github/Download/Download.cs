using System.IO.Abstractions;
using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Migrations;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class Download : IDownload
{
    private readonly GlobalArgs _globalArgs;
    private readonly DownloadArgs _downloadArgs;
    private readonly IMigrationService _migrationService;
    private readonly ILoginService _loginService;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<Download> _logger;

    public Download(
        GlobalArgs globalArgs,
        DownloadArgs downloadArgs,
        IMigrationService migrationService,
        ILoginService loginService,
        IFileSystem fileSystem,
        ILogger<Download> logger)
    {
        _globalArgs = globalArgs;
        _downloadArgs = downloadArgs;
        _migrationService = migrationService;
        _loginService = loginService;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var user = await _loginService.ValidateLoginAsync(ct);

        if (!_globalArgs.Quiet)
        {
            AnsiConsole.WriteLine($"Logged in as {user.Name}");
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

        if (!migrations.Any())
        {
            _logger.LogInformation("No migrations found");
            
            if (!_globalArgs.Quiet)
            {
                AnsiConsole.WriteLine("No migrations found");
            }
            
            return;
        }
        
        var migration = migrations.OrderBy(m => m.CreatedAt).Last();
        await DownloadMigrationAsync(migration.Id, ct);
    }

    private async Task DownloadUsingIdsAsync(CancellationToken ct)
    {
        foreach (var id in _downloadArgs.Migrations)
        {
            await DownloadMigrationAsync(id, ct);
        }
    }

    private async Task DownloadMigrationAsync(long id, CancellationToken ct)
    {
        _logger.LogInformation("Downloading migration {Id}", id);
        
        var options = new DownloadMigrationOptions(
            id,
            _fileSystem.DirectoryInfo.Wrap(_downloadArgs.Destination),
            _downloadArgs.NumberOfBackups,
            _downloadArgs.Overwrite
        );

        var migration = await _migrationService.GetMigrationAsync(id, ct);

        if (migration.State != MigrationState.Exported)
        {
            _logger.LogInformation("Migration {Id} is not yet exported - skipping", id);
            return;
        }

        if (!_globalArgs.Quiet)
        {
            AnsiConsole.WriteLine($"Downloading migration {migration.Id} to {_downloadArgs.Destination}...");
        }

        var path = await _migrationService.DownloadMigrationAsync(options, ct);
        _logger.LogInformation("Downloaded migration {Id} to {Path}", id, path);

        if (!_globalArgs.Quiet)
        {
            AnsiConsole.WriteLine($"Downloaded migration {migration.Id} to {path}");
        }
    }
}