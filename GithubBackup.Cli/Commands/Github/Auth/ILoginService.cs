using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Users;

namespace GithubBackup.Cli.Commands.Github.Auth;

internal interface ILoginService
{
    public Task<User?> TryLoginWithTemporaryTokenAsync(
        GlobalArgs globalArgs,
        LoginArgs args,
        CancellationToken ct
    );

    public Task<User> LoginAsync(GlobalArgs globalArgs, LoginArgs args, CancellationToken ct);
}
