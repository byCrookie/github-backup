using System.IO.Abstractions;
using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Options;
using GithubBackup.Cli.Utils;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Repositories;
using GithubBackup.Core.Github.Users;
using Polly;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal sealed class Backup : IBackup
{
    private readonly GlobalArgs _globalArgs;
    private readonly BackupArgs _backupArgs;
    private readonly IAuthenticationService _authenticationService;
    private readonly IMigrationService _migrationService;
    private readonly IUserService _userService;
    private readonly IRepositoryService _repositoryService;
    private readonly ICredentialStore _credentialStore;
    private readonly ILoginService _loginService;
    private readonly IFileSystem _fileSystem;

    public Backup(
        GlobalArgs globalArgs,
        BackupArgs backupArgs,
        IAuthenticationService authenticationService,
        IMigrationService migrationService,
        IUserService userService,
        IRepositoryService repositoryService,
        ICredentialStore credentialStore,
        ILoginService loginService,
        IFileSystem fileSystem)
    {
        _globalArgs = globalArgs;
        _backupArgs = backupArgs;
        _authenticationService = authenticationService;
        _migrationService = migrationService;
        _userService = userService;
        _repositoryService = repositoryService;
        _credentialStore = credentialStore;
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
            _backupArgs.MigrateArgs.ExcludeMetadataOnly
        );

        var migration = await _migrationService.StartMigrationAsync(options, ct);

        if (!_globalArgs.Quiet)
        {
            AnsiConsole.WriteLine($"Migration started with id {migration.Id}");
        }

        await Policy
            .HandleResult<Migration>(e => e.State != MigrationState.Exported)
            .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            .ExecuteAsync(async () =>
            {
                var migrationStatus = await _migrationService.GetMigrationAsync(migration.Id, ct);

                if (!_globalArgs.Quiet)
                {
                    AnsiConsole.WriteLine($"Migration {migration.Id} is {migrationStatus.State}");
                }

                return migrationStatus;
            });

        var downloadOptions = new DownloadMigrationOptions(
            migration.Id,
            _backupArgs.DownloadArgs.Destination,
            _backupArgs.DownloadArgs.NumberOfBackups,
            _backupArgs.DownloadArgs.Overwrite
        );

        if (!_globalArgs.Quiet)
        {
            AnsiConsole.WriteLine($"Downloading migration {migration.Id} to {_backupArgs.DownloadArgs.Destination}...");
        }

        var file = await _migrationService.DownloadMigrationAsync(downloadOptions, ct);

        if (!_globalArgs.Quiet)
        {
            AnsiConsole.WriteLine($"Downloaded migration {migration.Id} ({file})");
        }

        AnsiConsole.WriteLine(file);
    }
}