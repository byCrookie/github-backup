using System.IO.Abstractions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Repositories;
using GithubBackup.Core.Github.Users;
using GithubBackup.Core.Utils;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Manual;

internal sealed class ManualBackupRunner : ICommandRunner
{
    private readonly GlobalArgs _globalArgs;
    private readonly IMigrationService _migrationService;
    private readonly IRepositoryService _repositoryService;
    private readonly IFileSystem _fileSystem;
    private readonly IAnsiConsole _ansiConsole;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILoginService _loginService;

    public ManualBackupRunner(
        // Needs to be passed in because of the way ICommand's are resolved in
        // the cli service.
        // ReSharper disable once UnusedParameter.Local
        GlobalArgs globalArgs,
        // ReSharper disable once UnusedParameter.Local
        ManualBackupArgs _2,
        IMigrationService migrationService,
        IRepositoryService repositoryService,
        IFileSystem fileSystem,
        IAnsiConsole ansiConsole,
        IDateTimeProvider dateTimeProvider,
        ILoginService loginService)
    {
        _globalArgs = globalArgs;
        _migrationService = migrationService;
        _repositoryService = repositoryService;
        _fileSystem = fileSystem;
        _ansiConsole = ansiConsole;
        _dateTimeProvider = dateTimeProvider;
        _loginService = loginService;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        await LoginAsync(ct);

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

            _ansiConsole.WriteLine($"Found {migrations.Count} migrations:");
            foreach (var migration in migrations)
            {
                _ansiConsole.WriteLine($"- {migration.Id} {migration.State} {migration.CreatedAt} ({(_dateTimeProvider.Now - migration.CreatedAt).Days}d)");
            }

            if (!migrations.Any(m => m.State == MigrationState.Exported && m.CreatedAt > _dateTimeProvider.Now.AddDays(-7)))
            {
                _ansiConsole.WriteLine("No exported migrations found in the last 7 days.");
                return;
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
                    .AddChoices(migrations.Where(m => m.State == MigrationState.Exported && m.CreatedAt > _dateTimeProvider.Now.AddDays(-7)))
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

    private async Task LoginAsync(CancellationToken ct)
    {
        try
        {
            var user = await _loginService.PersistentOnlyAsync(_globalArgs, new LoginArgs(null, false), ct);

            if (user is not null && _ansiConsole.Confirm($"Do you want to continue as {user.Name}?"))
            {
                return;
            }

            await LoginInternAsync(ct);
        }
        catch (Exception)
        {
            await LoginInternAsync(ct);
        }
    }

    private Task<User> LoginInternAsync(CancellationToken ct)
    {
        var useDeviceFlowAuth = _ansiConsole.Confirm("Do you want to login using device flow authentication?");

        if (useDeviceFlowAuth)
        {
            return _loginService.WithoutPersistentAsync(
                new GlobalArgs(_globalArgs.Verbosity, false, _globalArgs.LogFile),
                new LoginArgs(null, true),
                true,
                ct
            );
        }

        var token = _ansiConsole.Ask<string>("Please enter your Github token:");
        return _loginService.WithoutPersistentAsync(
            new GlobalArgs(_globalArgs.Verbosity, false, _globalArgs.LogFile),
            new LoginArgs(token, false),
            true,
            ct
        );
    }
}