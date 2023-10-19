using System.IO.Abstractions;
using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Utils;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Repositories;
using GithubBackup.Core.Github.Users;
using GithubBackup.Core.Utils;
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
    private readonly IAnsiConsole _ansiConsole;
    private readonly IDateTimeProvider _dateTimeProvider;

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
        IFileSystem fileSystem,
        IAnsiConsole ansiConsole,
        IDateTimeProvider dateTimeProvider)
    {
        _authenticationService = authenticationService;
        _migrationService = migrationService;
        _userService = userService;
        _repositoryService = repositoryService;
        _credentialStore = credentialStore;
        _fileSystem = fileSystem;
        _ansiConsole = ansiConsole;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var user = await LoginAsync(ct);
        _ansiConsole.WriteLine($"Logged in as {user.Name}");

        if (_ansiConsole.Confirm("Do you want to start a migration?", false))
        {
            var byType = _ansiConsole.Confirm("Do you want to select repositories by type? If selected, no affiliation or visibility can be selected.", false);
            var type = (RepositoryType?)null;
            var affiliation = (RepositoryAffiliation?)RepositoryAffiliation.Owner;
            var visibility = (RepositoryVisibility?)RepositoryVisibility.All;

            switch (byType)
            {
                case true:
                    type = _ansiConsole.Prompt(new SelectionPrompt<RepositoryType>()
                        .Title("What type of repositories do you want to backup?")
                        .PageSize(20)
                        .AddChoices(Enum.GetValues<RepositoryType>()));
                    break;
                case false:
                    affiliation = _ansiConsole.Prompt(new SelectionPrompt<RepositoryAffiliation>()
                        .Title("Which affiliation type do you want to backup?")
                        .PageSize(20)
                        .AddChoices(Enum.GetValues<RepositoryAffiliation>()));
                
                    visibility = _ansiConsole.Prompt(new SelectionPrompt<RepositoryVisibility>()
                        .Title("Which visibility type do you want to backup?")
                        .PageSize(20)
                        .AddChoices(Enum.GetValues<RepositoryVisibility>()));
                    break;
            }

            var repositoryOptions = new RepositoryOptions(type, affiliation, visibility);
            
            var repositories = await _repositoryService.GetRepositoriesAsync(repositoryOptions, ct);

            if (!repositories.Any())
            {
                _ansiConsole.WriteLine("No repositories found.");
                return;
            }

            var selectedRepositories = _ansiConsole.Prompt(
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

            var selectedOptions = _ansiConsole.Prompt(
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
                _ansiConsole.WriteLine("No migrations found.");
                return;
            }

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
                    .Title("Select [green]migrations[/] to download?")
                    .Required()
                    .PageSize(20)
                    .MoreChoicesText("(Move up and down to reveal more migrations)")
                    .InstructionsText(
                        "(Press [blue]<space>[/] to toggle a migration, " +
                        "[green]<enter>[/] to accept)")
                    .AddChoices(migrationStatus.Where(m => m.State == MigrationState.Exported && m.CreatedAt > _dateTimeProvider.Now.AddDays(-7)))
                    .UseConverter(m => $"{m.Id} {m.State} {m.CreatedAt} ({(_dateTimeProvider.Now - m.CreatedAt).Days}d)")
            );

            var destination = _ansiConsole.Ask<string>("Where do you want to save the migration files?");

            while (!_fileSystem.Directory.Exists(destination))
            {
                destination = _ansiConsole.Ask<string>("The destination directory does not exist. Please enter a valid directory.");
            }

            foreach (var migration in selectedMigrations)
            {
                _ansiConsole.WriteLine($"Downloading migration {migration.Id} to {destination}...");
                var file = await _migrationService.DownloadMigrationAsync(new DownloadMigrationOptions(migration.Id, _fileSystem.DirectoryInfo.New(destination)), ct);
                _ansiConsole.WriteLine($"Downloaded migration {migration.Id} ({file})");
            }
        } while (_ansiConsole.Confirm("Fetch migration status again?"));
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
                if (_ansiConsole.Confirm($"Do you want to continue as {user.Name}?"))
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
        _ansiConsole.WriteLine(
            $"Go to {deviceAndUserCodes.VerificationUri}{Environment.NewLine}and enter {deviceAndUserCodes.UserCode}");
        _ansiConsole.WriteLine($"You have {deviceAndUserCodes.ExpiresIn} seconds to authenticate before the code expires.");
        var accessToken = await _authenticationService.PollForAccessTokenAsync(
            deviceAndUserCodes.DeviceCode,
            deviceAndUserCodes.Interval,
            ct
        );
        await _credentialStore.StoreTokenAsync(accessToken.Token, ct);
        return accessToken.Token;
    }
}