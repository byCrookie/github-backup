using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Options;
using GithubBackup.Core.Github.Migrations;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class Download : IDownload
{
    private readonly GlobalArgs _globalArgs;
    private readonly DownloadArgs _downloadArgs;
    private readonly IMigrationService _migrationService;
    private readonly ILoginService _loginService;

    public Download(
        GlobalArgs globalArgs,
        DownloadArgs downloadArgs,
        IMigrationService migrationService,
        ILoginService loginService)
    {
        _globalArgs = globalArgs;
        _downloadArgs = downloadArgs;
        _migrationService = migrationService;
        _loginService = loginService;
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
            await DownloadLatestAsync(ct);
            return;
        }

        await DownloadUsingIdsAsync(ct);
    }

    private async Task DownloadLatestAsync(CancellationToken ct)
    {
        var migrations = await _migrationService.GetMigrationsAsync(ct);

        if (!migrations.Any())
        {
            if (!_globalArgs.Quiet)
            {
                AnsiConsole.WriteLine("No migrations found.");
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
        var options = new DownloadMigrationOptions(
            id,
            _downloadArgs.Destination,
            _downloadArgs.NumberOfBackups,
            _downloadArgs.Overwrite
        );

        var migration = await _migrationService.GetMigrationAsync(id, ct);

        if (migration.State != MigrationState.Exported)
        {
            return;
        }

        if (!_globalArgs.Quiet)
        {
            AnsiConsole.WriteLine($"Downloading migration {migration.Id} to {_downloadArgs.Destination}...");
        }

        var path = await _migrationService.DownloadMigrationAsync(options, ct);

        if (!_globalArgs.Quiet)
        {
            AnsiConsole.WriteLine($"Downloaded migration {migration.Id} to {path}.");
        }
    }
}