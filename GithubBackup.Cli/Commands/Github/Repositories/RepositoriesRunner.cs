using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Output;
using GithubBackup.Core.Github.Repositories;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal sealed class RepositoriesRunner(
    GlobalArgs globalArgs,
    RepositoriesArgs repositoriesArgs,
    IRepositoryService repositoryService,
    ILoginService loginService,
    ICliOutput output
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
            output.Status("No repositories found.");

            return;
        }

        output.Status($"Found {repositories.Count} repositories:");
        foreach (var repository in repositories)
        {
            output.Status($"- {repository.FullName}");
        }

        output.Data(string.Join(" ", repositories.Select(r => r.FullName)));
    }
}
