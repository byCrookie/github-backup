using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Output;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class MigrationsRunner(
    GlobalArgs globalArgs,
    MigrationsArgs migrationsArgs,
    IMigrationService migrationService,
    ILoginService loginService,
    ICliOutput output,
    IDateTimeProvider dateTimeProvider
) : ICommandRunner
{
    public async Task RunAsync(CancellationToken ct)
    {
        await loginService.LoginAsync(globalArgs, migrationsArgs.LoginArgs, ct);

        var migrations = await migrationService.GetMigrationsAsync(ct);

        if (migrations.Count == 0)
        {
            output.Status("No migrations found.");

            return;
        }

        var filteredMigrations = migrations
            .Where(m =>
                !migrationsArgs.Export
                || (
                    m.State == MigrationState.Exported
                    && (dateTimeProvider.Now - m.CreatedAt).Days <= 7
                )
            )
            .Where(m =>
                migrationsArgs.DaysOld is null
                || (dateTimeProvider.Now - m.CreatedAt).Days <= migrationsArgs.DaysOld
            )
            .Where(m => migrationsArgs.Since is null || m.CreatedAt >= migrationsArgs.Since)
            .ToList();

        if (filteredMigrations.Count == 0)
        {
            output.Status("No migrations found after filters were applied.");

            return;
        }

        output.Status($"Found {filteredMigrations.Count} migrations:");
        foreach (var migration in filteredMigrations)
        {
            output.Status(
                $"- {migration.Id} {migration.State} {migration.CreatedAt} ({(dateTimeProvider.Now - migration.CreatedAt).Days}d)"
            );
        }

        output.Data(string.Join(" ", filteredMigrations.Select(m => m.Id)));
    }
}
