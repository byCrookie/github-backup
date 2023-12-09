using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Utils;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class MigrationsRunner : ICommandRunner
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
        await _loginService.WithPersistentAsync(
            _globalArgs,
            _migrationsArgs.LoginArgs,
            false,
            ct
        );

        var migrations = await _migrationService.GetMigrationsAsync(ct);

        if (!migrations.Any())
        {
            if (!_globalArgs.Quiet)
            {
                _ansiConsole.WriteLine("No migrations found.");
            }

            return;
        }

        var filteredMigrations = migrations
            .Where(m => !_migrationsArgs.Export || (m.State == MigrationState.Exported && (_dateTimeProvider.Now - m.CreatedAt).Days <= 7))
            .Where(m => _migrationsArgs.DaysOld is null || (_dateTimeProvider.Now - m.CreatedAt).Days <= _migrationsArgs.DaysOld)
            .Where(m => _migrationsArgs.Since is null || m.CreatedAt >= _migrationsArgs.Since)
            .ToList();

        if (!filteredMigrations.Any())
        {
            if (!_globalArgs.Quiet)
            {
                _ansiConsole.WriteLine("No migrations found after filters were applied.");
            }

            return;
        }

        if (!_globalArgs.Quiet)
        {
            _ansiConsole.WriteLine($"Found {filteredMigrations.Count} migrations:");
            foreach (var migration in filteredMigrations)
            {
                _ansiConsole.WriteLine($"- {migration.Id} {migration.State} {migration.CreatedAt} ({(_dateTimeProvider.Now - migration.CreatedAt).Days}d)");
            }
        }

        _ansiConsole.WriteLine(string.Join(" ", filteredMigrations.Select(m => m.Id)));
    }
}