using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Repositories;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal sealed class RepositoriesRunner : IRepositoriesRunner
{
    private readonly GlobalArgs _globalArgs;
    private readonly RepositoriesArgs _repositoriesArgs;
    private readonly IRepositoryService _repositoryService;
    private readonly ILoginService _loginService;

    public RepositoriesRunner(
        GlobalArgs globalArgs,
        RepositoriesArgs repositoriesArgs,
        IRepositoryService repositoryService,
        ILoginService loginService)
    {
        _globalArgs = globalArgs;
        _repositoriesArgs = repositoriesArgs;
        _repositoryService = repositoryService;
        _loginService = loginService;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var user = await _loginService.ValidateLoginAsync(ct);

        if (!_globalArgs.Quiet)
        {
            AnsiConsole.WriteLine($"Logged in as {user.Name}");
        }

        var options = new RepositoryOptions(
            _repositoriesArgs.Type,
            _repositoriesArgs.Affiliation,
            _repositoriesArgs.Visibility
        );

        var repositories = await _repositoryService.GetRepositoriesAsync(options, ct);

        if (!repositories.Any())
        {
            if (!_globalArgs.Quiet)
            {
                AnsiConsole.WriteLine("No migrations found.");
            }

            return;
        }

        if (!_globalArgs.Interactive)
        {
            AnsiConsole.WriteLine($"Found {repositories.Count} repositories:");
            foreach (var repository in repositories)
            {
                AnsiConsole.WriteLine($"- {repository.FullName}");
            }
            
            AnsiConsole.WriteLine(string.Join(" ", repositories.Select(r => r.FullName)));
        }
        else
        {
            var selectedRepositories = AnsiConsole.Prompt(
                new MultiSelectionPrompt<Repository>()
                    .Title("Select [green]repositories[/] to print?")
                    .Required(false)
                    .PageSize(20)
                    .MoreChoicesText("(Move up and down to reveal more repositories)")
                    .InstructionsText(
                        "(Press [blue]<space>[/] to toggle a repository, " +
                        "[green]<enter>[/] to accept)")
                    .AddChoices(repositories)
                    .UseConverter(r => r.FullName)
            );

            AnsiConsole.WriteLine(string.Join(" ", selectedRepositories.Select(r => r.FullName)));   
        }
    }
}