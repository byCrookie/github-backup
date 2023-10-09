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

        switch (_migrationsArgs.Id)
        {
            case false:
            {
                var migrationStatus = await migrations
                    .SelectAsync(m => _migrationService.GetMigrationAsync(m.Id, ct))
                    .ToListAsync(cancellationToken: ct);
                
                AnsiConsole.WriteLine($"Found {migrationStatus.Count} migrations:");
                foreach (var migration in migrationStatus)
                {
                    AnsiConsole.WriteLine($"- {migration.Id} ({migration.State})");
                }

                break;
            }
            case true:
                AnsiConsole.WriteLine(string.Join(" ", migrations.Select(m => m.Id)));
                break;
        }
    }
}