using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Migrations;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal sealed class MigrateRunner(
    GlobalArgs globalArgs,
    MigrateArgs migrateArgs,
    IMigrationService migrationService,
    ILoginService loginService,
    IAnsiConsole ansiConsole
) : ICommandRunner
{
    public async Task RunAsync(CancellationToken ct)
    {
        await loginService.LoginAsync(globalArgs, migrateArgs.LoginArgs, ct);

        var options = new StartMigrationOptions(
            migrateArgs.Repositories,
            migrateArgs.LockRepositories,
            migrateArgs.ExcludeMetadata,
            migrateArgs.ExcludeGitData,
            migrateArgs.ExcludeAttachements,
            migrateArgs.ExcludeReleases,
            migrateArgs.ExcludeOwnerProjects,
            migrateArgs.OrgMetadataOnly
        );

        var migration = await migrationService.StartMigrationAsync(options, ct);

        ansiConsole.WriteLine(
            !globalArgs.Quiet ? $"Migration started with id {migration.Id}" : $"{migration.Id}"
        );
    }
}
