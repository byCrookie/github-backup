using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Options;
using GithubBackup.Cli.Utils;
using GithubBackup.Core.Github.Migrations;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class Migrations : IMigrations
{
    private readonly GlobalArgs _globalArgs;
    private readonly MigrationsArgs _migrationsArgs;
    private readonly IMigrationService _migrationService;
    private readonly ILoginService _loginService;

    public Migrations(
        GlobalArgs globalArgs,
        MigrationsArgs migrationsArgs,
        IMigrationService migrationService,
        ILoginService loginService)
    {
        _globalArgs = globalArgs;
        _migrationsArgs = migrationsArgs;
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

        var migrations = await _migrationService.GetMigrationsAsync(ct);

        if (!migrations.Any())
        {
            if (!_globalArgs.Quiet)
            {
                AnsiConsole.WriteLine("No migrations found.");
            }

            return;
        }

        if (_globalArgs.Interactive)
        {
            var migrationStatus = await migrations
                .SelectAsync(m => _migrationService.GetMigrationAsync(m.Id, ct))
                .ToListAsync(cancellationToken: ct);

            AnsiConsole.WriteLine($"Found {migrationStatus.Count} migrations:");
            foreach (var migration in migrationStatus)
            {
                AnsiConsole.WriteLine($"- {migration.Id} ({migration.State})");
            }

            var selectedMigrations = AnsiConsole.Prompt(
                new MultiSelectionPrompt<Migration>()
                    .Title("Select [green]migrations[/] to print?")
                    .Required()
                    .PageSize(20)
                    .MoreChoicesText("(Move up and down to reveal more migrations)")
                    .InstructionsText(
                        "(Press [blue]<space>[/] to toggle a migration, " +
                        "[green]<enter>[/] to accept)")
                    .AddChoices(migrationStatus.Where(m => m.State == MigrationState.Exported))
                    .UseConverter(m => $"{m.Id} ({m.State})")
            );

            AnsiConsole.WriteLine(string.Join(" ", selectedMigrations.Select(m => m.Id)));
            return;
        }

        if (_migrationsArgs.Id)
        {
            AnsiConsole.WriteLine(string.Join(" ", migrations.Select(m => m.Id)));
        }
        else
        {
            var migrationStatus = await migrations
                .SelectAsync(m => _migrationService.GetMigrationAsync(m.Id, ct))
                .ToListAsync(cancellationToken: ct);

            AnsiConsole.WriteLine($"Found {migrationStatus.Count} migrations:");
            foreach (var migration in migrationStatus)
            {
                AnsiConsole.WriteLine($"- {migration.Id} ({migration.State})");
            }
        }
    }
}