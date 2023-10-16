using System.IO.Abstractions;
using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Options;
using GithubBackup.Core.Github.Migrations;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal sealed class Backup : IBackup
{
    private readonly GlobalArgs _globalArgs;
    private readonly BackupArgs _backupArgs;
    private readonly IMigrationService _migrationService;
    private readonly ILoginService _loginService;
    private readonly IFileSystem _fileSystem;

    public Backup(
        GlobalArgs globalArgs,
        BackupArgs backupArgs,
        IMigrationService migrationService,
        ILoginService loginService,
        IFileSystem fileSystem)
    {
        _globalArgs = globalArgs;
        _backupArgs = backupArgs;
        _migrationService = migrationService;
        _loginService = loginService;
        _fileSystem = fileSystem;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var user = await _loginService.ValidateLoginAsync(ct);

        if (!_globalArgs.Quiet)
        {
            AnsiConsole.WriteLine($"Logged in as {user.Name}");
        }

        var options = new StartMigrationOptions(
            _backupArgs.MigrateArgs.Repositories,
            _backupArgs.MigrateArgs.LockRepositories,
            _backupArgs.MigrateArgs.ExcludeMetadata,
            _backupArgs.MigrateArgs.ExcludeGitData,
            _backupArgs.MigrateArgs.ExcludeAttachements,
            _backupArgs.MigrateArgs.ExcludeReleases,
            _backupArgs.MigrateArgs.ExcludeOwnerProjects,
            _backupArgs.MigrateArgs.OrgMetadataOnly
        );
        
        var migration = await _migrationService.StartMigrationAsync(options, ct);

        if (!_globalArgs.Quiet)
        {
            AnsiConsole.WriteLine(
                $"Downloading migration {migration.Id} to {_backupArgs.DownloadArgs.Destination} when ready...");
        }

        var downloadOptions = new DownloadMigrationOptions(
            migration.Id,
            _fileSystem.DirectoryInfo.Wrap(_backupArgs.DownloadArgs.Destination),
            _backupArgs.DownloadArgs.NumberOfBackups,
            _backupArgs.DownloadArgs.Overwrite
        );

        var file = await _migrationService.PollAndDownloadMigrationAsync(
            downloadOptions,
            update =>
            {
                AnsiConsole.WriteLine($"Migration {update.Id} is {update.State}...");
                return Task.CompletedTask;
            },
            ct
        );
        
        AnsiConsole.WriteLine(!_globalArgs.Quiet ? $"Downloaded migration {migration.Id} ({file})" : file);
    }
}