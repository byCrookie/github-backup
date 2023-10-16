using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Options;
using GithubBackup.Core.Github.Migrations;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal sealed class Migrate : IMigrate
{
    private readonly GlobalArgs _globalArgs;
    private readonly MigrateArgs _migrateArgs;
    private readonly IMigrationService _migrationService;
    private readonly ILoginService _loginService;

    public Migrate(
        GlobalArgs globalArgs,
        MigrateArgs migrateArgs,
        IMigrationService migrationService,
        ILoginService loginService)
    {
        _globalArgs = globalArgs;
        _migrateArgs = migrateArgs;
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

        if (_globalArgs.Interactive && !AnsiConsole.Confirm("Do you want to start a migration?", false))
        {
            return;
        }

        var options = new StartMigrationOptions(
            _migrateArgs.Repositories,
            _migrateArgs.LockRepositories,
            _migrateArgs.ExcludeMetadata,
            _migrateArgs.ExcludeGitData,
            _migrateArgs.ExcludeAttachements,
            _migrateArgs.ExcludeReleases,
            _migrateArgs.ExcludeOwnerProjects,
            _migrateArgs.OrgMetadataOnly
        );
        
        var migration = await _migrationService.StartMigrationAsync(options, ct);

        if (!_globalArgs.Quiet)
        {
            AnsiConsole.WriteLine($"Migration started with id {migration.Id}");
        }
    }
}