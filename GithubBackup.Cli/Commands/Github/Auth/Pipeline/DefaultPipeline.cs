using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Users;

namespace GithubBackup.Cli.Commands.Github.Auth.Pipeline;

internal class DefaultPipeline : IDefaultPipeline
{
    public ILoginPipeline? Next { get; set; }

    public Task<User?> LoginAsync(
        GlobalArgs globalArgs,
        LoginArgs args,
        bool persist,
        CancellationToken ct
    )
    {
        return Task.FromResult<User?>(null);
    }
}
