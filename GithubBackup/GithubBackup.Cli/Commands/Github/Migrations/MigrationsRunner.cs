using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Utils;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class MigrationsRunner : IMigrationsRunner
{
    private readonly GlobalArgs _globalArgs;
    private readonly MigrationsArgs _migrationsArgs;
    private readonly IMigrationService _migrationService;
    private readonly ILoginService _loginService;
    private readonly IAnsiConsole _ansiConsole;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MigrationsRunner(
        GlobalArgs globalArgs,
        MigrationsArgs migrationsArgs,
        IMigrationService migrationService,
        ILoginService loginService,
        IAnsiConsole ansiConsole,
        IDateTimeProvider dateTimeProvider)
    {
        _globalArgs = globalArgs;
        _migrationsArgs = migrationsArgs;
        _migrationService = migrationService;
        _loginService = loginService;
        _ansiConsole = ansiConsole;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var user = await _loginService.ValidateLoginAsync(ct);

        if (!_globalArgs.Quiet)
        {
            _ansiConsole.WriteLine($"Logged in as {user.Name}");
        }

        var migrations = await _migrationService.GetMigrationsAsync(ct);

        if (!migrations.Any())
        {
            if (!_globalArgs.Quiet)
            {
                _ansiConsole.WriteLine("No migrations found.");
            }

            return;
        }

        if (_globalArgs.Interactive)
        {
            var migrationStatus = await migrations
                .SelectAsync(m => _migrationService.GetMigrationAsync(m.Id, ct))
                .ToListAsync(cancellationToken: ct);

            _ansiConsole.WriteLine($"Found {migrationStatus.Count} migrations:");
            foreach (var migration in migrationStatus)
            {
                _ansiConsole.WriteLine($"- {migration.Id} {migration.State} {migration.CreatedAt} ({(_dateTimeProvider.Now - migration.CreatedAt).Days}d)");
            }

            var selectedMigrations = _ansiConsole.Prompt(
                new MultiSelectionPrompt<Migration>()
                    .Title("Select [green]migrations[/] to print?")
                    .Required()
                    .PageSize(20)
                    .MoreChoicesText("(Move up and down to reveal more migrations)")
                    .InstructionsText(
                        "(Press [blue]<space>[/] to toggle a migration, " +
                        "[green]<enter>[/] to accept)")
                    .AddChoices(migrationStatus.Where(m => m.State == MigrationState.Exported && m.CreatedAt > _dateTimeProvider.Now.AddDays(-7)))
                    .UseConverter(m => $"{m.Id} {m.State} {m.CreatedAt} ({(_dateTimeProvider.Now - m.CreatedAt).Days}d)")
            );

            _ansiConsole.WriteLine(string.Join(" ", selectedMigrations.Select(m => m.Id)));
            return;
        }

        if (_migrationsArgs.Long)
        {
            _ansiConsole.WriteLine(string.Join(" ", migrations.Select(m => m.Id)));
        }
        else
        {
            var migrationStatus = await migrations
                .SelectAsync(m => _migrationService.GetMigrationAsync(m.Id, ct))
                .ToListAsync(cancellationToken: ct);

            _ansiConsole.WriteLine($"Found {migrationStatus.Count} migrations:");
            foreach (var migration in migrationStatus)
            {
                _ansiConsole.WriteLine($"- {migration.Id} {migration.State} {migration.CreatedAt} ({(_dateTimeProvider.Now - migration.CreatedAt).Days}d)");
            }

            _ansiConsole.WriteLine(string.Join(" ", migrations.Select(m => m.Id)));
        }
    }
}