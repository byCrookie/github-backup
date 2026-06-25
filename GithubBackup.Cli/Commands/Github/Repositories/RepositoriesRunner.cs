using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Repositories;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal sealed class RepositoriesRunner(
    GlobalArgs globalArgs,
    RepositoriesArgs repositoriesArgs,
    IRepositoryService repositoryService,
    ILoginService loginService,
    IAnsiConsole ansiConsole
) : ICommandRunner
{
    public async Task RunAsync(CancellationToken ct)
    {
        await loginService.LoginAsync(globalArgs, repositoriesArgs.LoginArgs, ct);

        var options = new RepositoryOptions(
            repositoriesArgs.Type,
            repositoriesArgs.Affiliation,
            repositoriesArgs.Visibility
        );

        var repositories = await repositoryService.GetRepositoriesAsync(options, ct);

        if (repositories.Count == 0)
        {
            if (!globalArgs.Quiet)
            {
                ansiConsole.WriteLine("No migrations found.");
            }

            return;
        }

        if (!globalArgs.Quiet)
        {
            ansiConsole.WriteLine($"Found {repositories.Count} repositories:");
            foreach (var repository in repositories)
            {
                ansiConsole.WriteLine($"- {repository.FullName}");
            }
        }

        ansiConsole.WriteLine(string.Join(" ", repositories.Select(r => r.FullName)));
    }
}
