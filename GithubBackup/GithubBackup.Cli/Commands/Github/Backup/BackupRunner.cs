using System.IO.Abstractions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Migrations;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal sealed class BackupRunner : IBackupRunner
{
    private readonly GlobalArgs _globalArgs;
    private readonly BackupArgs _backupArgs;
    private readonly IMigrationService _migrationService;
    private readonly ILoginService _loginService;
    private readonly IFileSystem _fileSystem;
    private readonly IAnsiConsole _ansiConsole;

    public BackupRunner(
        GlobalArgs globalArgs,
        BackupArgs backupArgs,
        IMigrationService migrationService,
        ILoginService loginService,
        IFileSystem fileSystem,
        IAnsiConsole ansiConsole)
    {
        _globalArgs = globalArgs;
        _backupArgs = backupArgs;
        _migrationService = migrationService;
        _loginService = loginService;
        _fileSystem = fileSystem;
        _ansiConsole = ansiConsole;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        await _loginService.WithPersistentAsync(
            _globalArgs,
            _backupArgs.LoginArgs,
            false,
            ct
        );

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
            _ansiConsole.WriteLine(
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
                if (!_globalArgs.Quiet)
                {
                    _ansiConsole.WriteLine($"Migration {update.Id} is {update.State}...");
                }

                return Task.CompletedTask;
            },
            ct
        );

        _ansiConsole.WriteLine(!_globalArgs.Quiet ? $"Downloaded migration {migration.Id} ({file})" : file);
    }
}