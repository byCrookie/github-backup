using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Repositories;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal sealed class RepositoriesRunner : IRepositoriesRunner
{
    private readonly GlobalArgs _globalArgs;
    private readonly RepositoriesArgs _repositoriesArgs;
    private readonly IRepositoryService _repositoryService;
    private readonly ILoginService _loginService;
    private readonly IAnsiConsole _ansiConsole;
    private readonly IGithubTokenStore _githubTokenStore;

    public RepositoriesRunner(
        GlobalArgs globalArgs,
        RepositoriesArgs repositoriesArgs,
        IRepositoryService repositoryService,
        ILoginService loginService,
        IAnsiConsole ansiConsole,
        IGithubTokenStore githubTokenStore)
    {
        _globalArgs = globalArgs;
        _repositoriesArgs = repositoriesArgs;
        _repositoryService = repositoryService;
        _loginService = loginService;
        _ansiConsole = ansiConsole;
        _githubTokenStore = githubTokenStore;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        await _loginService.LoginAsync(
            _globalArgs,
            _repositoriesArgs.LoginArgs,
            (token, _) => _githubTokenStore.SetAsync(token),
            ct
        );

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
                _ansiConsole.WriteLine("No migrations found.");
            }

            return;
        }

        if (!_globalArgs.Quiet)
        {
            _ansiConsole.WriteLine($"Found {repositories.Count} repositories:");
            foreach (var repository in repositories)
            {
                _ansiConsole.WriteLine($"- {repository.FullName}");
            }
        }

        _ansiConsole.WriteLine(string.Join(" ", repositories.Select(r => r.FullName)));
    }
}