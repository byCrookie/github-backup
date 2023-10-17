using System.IO.Abstractions;
using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Utils;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Repositories;
using GithubBackup.Core.Github.Users;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Manual;

internal sealed class ManualBackupRunner : IManualBackupRunner
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IMigrationService _migrationService;
    private readonly IUserService _userService;
    private readonly IRepositoryService _repositoryService;
    private readonly ICredentialStore _credentialStore;
    private readonly IFileSystem _fileSystem;

    public ManualBackupRunner(
        // Needs to be passed in because of the way ICommand's are resolved in
        // the cli service.
        // ReSharper disable once UnusedParameter.Local
        ManualBackupArgs _,
        IAuthenticationService authenticationService,
        IMigrationService migrationService,
        IUserService userService,
        IRepositoryService repositoryService,
        ICredentialStore credentialStore,
        IFileSystem fileSystem)
    {
        _authenticationService = authenticationService;
        _migrationService = migrationService;
        _userService = userService;
        _repositoryService = repositoryService;
        _credentialStore = credentialStore;
        _fileSystem = fileSystem;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var user = await LoginAsync(ct);
        AnsiConsole.WriteLine($"Logged in as {user.Name}");

        if (AnsiConsole.Confirm("Do you want to start a migration?", false))
        {
            var byType = AnsiConsole.Confirm("Do you want to select repositories by type? If selected, no affiliation or visibility can be selected.", false);
            var type = (RepositoryType?)null;
            var affiliation = (RepositoryAffiliation?)RepositoryAffiliation.Owner;
            var visibility = (RepositoryVisibility?)RepositoryVisibility.All;

            switch (byType)
            {
                case true:
                    type = AnsiConsole.Prompt(new SelectionPrompt<RepositoryType>()
                        .Title("What type of repositories do you want to backup?")
                        .PageSize(20)
                        .AddChoices(Enum.GetValues<RepositoryType>()));
                    break;
                case false:
                    affiliation = AnsiConsole.Prompt(new SelectionPrompt<RepositoryAffiliation>()
                        .Title("Which affiliation type do you want to backup?")
                        .PageSize(20)
                        .AddChoices(Enum.GetValues<RepositoryAffiliation>()));
                
                    visibility = AnsiConsole.Prompt(new SelectionPrompt<RepositoryVisibility>()
                        .Title("Which visibility type do you want to backup?")
                        .PageSize(20)
                        .AddChoices(Enum.GetValues<RepositoryVisibility>()));
                    break;
            }

            var repositoryOptions = new RepositoryOptions(type, affiliation, visibility);
            
            var repositories = await _repositoryService.GetRepositoriesAsync(repositoryOptions, ct);

            if (!repositories.Any())
            {
                AnsiConsole.WriteLine("No repositories found.");
                return;
            }

            var selectedRepositories = AnsiConsole.Prompt(
                new MultiSelectionPrompt<Repository>()
                    .Title("Select [green]repositories[/] to backup? If none is selected, all repositories will be backed up.")
                    .Required(false)
                    .PageSize(20)
                    .MoreChoicesText("(Move up and down to reveal more repositories)")
                    .InstructionsText(
                        "(Press [blue]<space>[/] to toggle a repository, " +
                        "[green]<enter>[/] to accept)")
                    .AddChoices(repositories)
                    .UseConverter(r => r.FullName)
            );

            var selectedOptions = AnsiConsole.Prompt(
                new MultiSelectionPrompt<Description>()
                    .Title("Select [green]options[/] to start migration?")
                    .PageSize(20)
                    .Required(false)
                    .MoreChoicesText("(Move up and down to reveal more options)")
                    .InstructionsText(
                        "(Press [blue]<space>[/] to toggle a option, " +
                        "[green]<enter>[/] to accept)")
                    .AddChoices(
                        MigrateArgDescriptions.LockRepositories,
                        MigrateArgDescriptions.ExcludeMetadata,
                        MigrateArgDescriptions.ExcludeGitData,
                        MigrateArgDescriptions.ExcludeAttachements,
                        MigrateArgDescriptions.ExcludeReleases,
                        MigrateArgDescriptions.ExcludeOwnerProjects,
                        MigrateArgDescriptions.OrgMetadataOnly
                    )
                    .UseConverter(d => $"{d.Display} - {d.Long}")
            );

            var options = new StartMigrationOptions(
                GetRepositoryNames(selectedRepositories, repositories),
                selectedOptions.Contains(MigrateArgDescriptions.LockRepositories),
                selectedOptions.Contains(MigrateArgDescriptions.ExcludeMetadata),
                selectedOptions.Contains(MigrateArgDescriptions.ExcludeGitData),
                selectedOptions.Contains(MigrateArgDescriptions.ExcludeAttachements),
                selectedOptions.Contains(MigrateArgDescriptions.ExcludeReleases),
                selectedOptions.Contains(MigrateArgDescriptions.ExcludeOwnerProjects),
                selectedOptions.Contains(MigrateArgDescriptions.OrgMetadataOnly)
            );

            await _migrationService.StartMigrationAsync(options, ct);
        }

        do
        {
            var migrations = await _migrationService.GetMigrationsAsync(ct);

            if (!migrations.Any())
            {
                AnsiConsole.WriteLine("No migrations found.");
                return;
            }

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
                    .Title("Select [green]migrations[/] to download?")
                    .Required()
                    .PageSize(20)
                    .MoreChoicesText("(Move up and down to reveal more migrations)")
                    .InstructionsText(
                        "(Press [blue]<space>[/] to toggle a migration, " +
                        "[green]<enter>[/] to accept)")
                    .AddChoices(migrationStatus.Where(m => m.State == MigrationState.Exported))
                    .UseConverter(m => $"{m.Id} ({m.State})")
            );

            var destination = AnsiConsole.Ask<string>("Where do you want to save the migration files?");

            while (!_fileSystem.Directory.Exists(destination))
            {
                destination = AnsiConsole.Ask<string>("The destination directory does not exist. Please enter a valid directory.");
            }

            foreach (var migration in selectedMigrations)
            {
                AnsiConsole.WriteLine($"Downloading migration {migration.Id} to {destination}...");
                var file = await _migrationService.DownloadMigrationAsync(new DownloadMigrationOptions(migration.Id, _fileSystem.DirectoryInfo.New(destination)), ct);
                AnsiConsole.WriteLine($"Downloaded migration {migration.Id} ({file})");
            }
        } while (AnsiConsole.Confirm("Fetch migration status again?"));
    }

    private static string[] GetRepositoryNames(IReadOnlyCollection<Repository> selectedRepositories, IEnumerable<Repository> repositories)
    {
        return selectedRepositories.Any() ? selectedRepositories.Select(r => r.FullName).ToArray() : repositories.Select(r => r.FullName).ToArray();
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
                var user = await _userService.WhoAmIAsync(ct);
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

        return await _userService.WhoAmIAsync(ct);
    }

    private async Task LoginAndStoreAsync(CancellationToken ct)
    {
        var token = await GetOAuthTokenAsync(ct);
        await _credentialStore.StoreTokenAsync(token, ct);
    }

    private async Task<string> GetOAuthTokenAsync(CancellationToken ct)
    {
        var deviceAndUserCodes = await _authenticationService.RequestDeviceAndUserCodesAsync(ct);
        AnsiConsole.WriteLine(
            $"Go to {deviceAndUserCodes.VerificationUri}{Environment.NewLine}and enter {deviceAndUserCodes.UserCode}");
        AnsiConsole.WriteLine($"You have {deviceAndUserCodes.ExpiresIn} seconds to authenticate before the code expires.");
        var accessToken = await _authenticationService.PollForAccessTokenAsync(
            deviceAndUserCodes.DeviceCode,
            deviceAndUserCodes.Interval,
            ct
        );
        await _credentialStore.StoreTokenAsync(accessToken.Token, ct);
        return accessToken.Token;
    }
}