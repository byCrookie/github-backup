using GithubBackup.Cli.Github.Credentials;
using GithubBackup.Cli.Options;
using GithubBackup.Cli.Utils;
using GithubBackup.Core.Github;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Repositories;
using GithubBackup.Core.Github.Users;
using Spectre.Console;

namespace GithubBackup.Cli.Github;

internal class Backup : IBackup
{
    private readonly GlobalArgs _globalArgs;
    private readonly GithubBackupArgs _backupArgs;
    private readonly IGithubService _githubService;
    private readonly ICredentialStore _credentialStore;

    public Backup(
        GlobalArgs globalArgs,
        GithubBackupArgs backupArgs,
        IGithubService githubService,
        ICredentialStore credentialStore)
    {
        _globalArgs = globalArgs;
        _backupArgs = backupArgs;
        _githubService = githubService;
        _credentialStore = credentialStore;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var user = await LoginAsync(ct);
        AnsiConsole.WriteLine($"Logged in as {user.Name}");

        if (AnsiConsole.Confirm("Do you want to start a migration?", false))
        {
            var repositories = await _githubService.GetRepositoriesAsync(ct);

            if (!repositories.Any())
            {
                AnsiConsole.WriteLine("No repositories found.");
                return;
            }

            var selectedRepositories = AnsiConsole.Prompt(
                new MultiSelectionPrompt<Repository>()
                    .Title("Select [green]repositories[/] to backup?")
                    .Required()
                    .PageSize(20)
                    .MoreChoicesText("(Move up and down to reveal more repositories)")
                    .InstructionsText(
                        "(Press [blue]<space>[/] to toggle a repository, " +
                        "[green]<enter>[/] to accept)")
                    .AddChoices(repositories)
                    .UseConverter(r => r.FullName)
            );

            var selectedRepositoryNames = selectedRepositories.Select(r => r.FullName).ToList();
            await _githubService.StartMigrationAsync(new StartMigrationOptions(selectedRepositoryNames), ct);
        }

        do
        {
            var migrations = await _githubService.GetMigrationsAsync(ct);

            if (!migrations.Any())
            {
                AnsiConsole.WriteLine("No migrations found.");
                return;
            }

            var migrationStatus = await migrations
                .SelectAsync(m => _githubService.GetMigrationAsync(m.Id, ct))
                .ToListAsync(cancellationToken: ct);

            AnsiConsole.WriteLine($"Found {migrationStatus.Count} migrations:");
            foreach (var migration in migrationStatus)
            {
                AnsiConsole.WriteLine($"- {migration.Id} ({migration.State})");
            }

            var selectedMigrations = AnsiConsole.Prompt(
                new MultiSelectionPrompt<Migration>()
                    .Title("Select [green]migrations[/] to download?")
                    .Required()
                    .PageSize(20)
                    .MoreChoicesText("(Move up and down to reveal more migrations)")
                    .InstructionsText(
                        "(Press [blue]<space>[/] to toggle a migration, " +
                        "[green]<enter>[/] to accept)")
                    .AddChoices(migrations.Where(m => m.State == MigrationState.Exported))
                    .UseConverter(m => $"{m.Id} ({m.State})")
            );

            foreach (var migration in selectedMigrations)
            {
                AnsiConsole.WriteLine($"Downloading migration {migration.Id} to {_backupArgs.Destination}...");
                var file = await _githubService.DownloadMigrationAsync(new DownloadMigrationOptions(migration.Id, _backupArgs.Destination), ct);
                AnsiConsole.WriteLine($"Downloaded migration {migration.Id} ({file})");
            }
        } while (AnsiConsole.Confirm("Fetch migration status again?"));
    }

    private async Task<User> LoginAsync(CancellationToken ct)
    {
        try
        {
            var token = await _credentialStore.LoadTokenAsync(ct);

            if (string.IsNullOrWhiteSpace(token))
            {
                await LoginAndStoreAsync(ct);
            }
            else
            {
                var user = await _githubService.WhoAmIAsync(ct);
                if (AnsiConsole.Confirm($"Do you want to continue as {user.Name}?"))
                {
                    return user;
                }

                await LoginAndStoreAsync(ct);
            }
        }
        catch (Exception)
        {
            await LoginAndStoreAsync(ct);
        }

        return await _githubService.WhoAmIAsync(ct);
    }

    private async Task LoginAndStoreAsync(CancellationToken ct)
    {
        var token = await GetOAuthTokenAsync(ct);
        await _credentialStore.StoreTokenAsync(token, ct);
    }

    private async Task<string> GetOAuthTokenAsync(CancellationToken ct)
    {
        var deviceAndUserCodes = await _githubService.RequestDeviceAndUserCodesAsync(ct);
        Console.WriteLine(
            $"Go to {deviceAndUserCodes.VerificationUri}{Environment.NewLine}and enter {deviceAndUserCodes.UserCode}");
        Console.WriteLine($"You have {deviceAndUserCodes.ExpiresIn} seconds to authenticate before the code expires.");
        var accessToken =
            await _githubService.PollForAccessTokenAsync(deviceAndUserCodes.DeviceCode, deviceAndUserCodes.Interval,
                ct);
        await _credentialStore.StoreTokenAsync(accessToken.Token, ct);
        return accessToken.Token;
    }
}