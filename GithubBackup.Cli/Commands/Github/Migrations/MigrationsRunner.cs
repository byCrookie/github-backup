using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Utils;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class MigrationsRunner(
    GlobalArgs globalArgs,
    MigrationsArgs migrationsArgs,
    IMigrationService migrationService,
    ILoginService loginService,
    IAnsiConsole ansiConsole,
    IDateTimeProvider dateTimeProvider
) : ICommandRunner
{
    public async Task RunAsync(CancellationToken ct)
    {
        await loginService.LoginAsync(globalArgs, migrationsArgs.LoginArgs, ct);

        var migrations = await migrationService.GetMigrationsAsync(ct);

        if (migrations.Count == 0)
        {
            if (!globalArgs.Quiet)
            {
                ansiConsole.WriteLine("No migrations found.");
            }

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
            if (!globalArgs.Quiet)
            {
                ansiConsole.WriteLine("No migrations found after filters were applied.");
            }

            return;
        }

        if (!globalArgs.Quiet)
        {
            ansiConsole.WriteLine($"Found {filteredMigrations.Count} migrations:");
            foreach (var migration in filteredMigrations)
            {
                ansiConsole.WriteLine(
                    $"- {migration.Id} {migration.State} {migration.CreatedAt} ({(dateTimeProvider.Now - migration.CreatedAt).Days}d)"
                );
            }
        }

        ansiConsole.WriteLine(string.Join(" ", filteredMigrations.Select(m => m.Id)));
    }
}
