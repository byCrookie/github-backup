using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Migrations;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal sealed class MigrateRunner : IMigrateRunner
{
    private readonly GlobalArgs _globalArgs;
    private readonly MigrateArgs _migrateArgs;
    private readonly IMigrationService _migrationService;
    private readonly ILoginService _loginService;
    private readonly IAnsiConsole _ansiConsole;
    private readonly IGithubTokenStore _githubTokenStore;

    public MigrateRunner(
        GlobalArgs globalArgs,
        MigrateArgs migrateArgs,
        IMigrationService migrationService,
        ILoginService loginService,
        IAnsiConsole ansiConsole,
        IGithubTokenStore githubTokenStore)
    {
        _globalArgs = globalArgs;
        _migrateArgs = migrateArgs;
        _migrationService = migrationService;
        _loginService = loginService;
        _ansiConsole = ansiConsole;
        _githubTokenStore = githubTokenStore;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        await _loginService.LoginAsync(
            _globalArgs,
            _migrateArgs.LoginArgs,
            (token, _) => _githubTokenStore.SetAsync(token),
            ct
        );

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
            _ansiConsole.WriteLine($"Migration started with id {migration.Id}");
        }
    }
}