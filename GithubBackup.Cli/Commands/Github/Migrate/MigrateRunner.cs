using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Output;
using GithubBackup.Core.Github.Migrations;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal sealed class MigrateRunner(
    GlobalArgs globalArgs,
    MigrateArgs migrateArgs,
    IMigrationService migrationService,
    ILoginService loginService,
    ICliOutput output
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

        output.Status($"Started migration {migration.Id}.");
        output.Data($"{migration.Id}");
    }
}
