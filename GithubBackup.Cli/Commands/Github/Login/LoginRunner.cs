using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;

namespace GithubBackup.Cli.Commands.Github.Login;

internal sealed class LoginRunner(
    GlobalArgs globalArgs,
    LoginArgs loginArgs,
    ILoginService loginService
) : ICommandRunner
{
    public Task RunAsync(CancellationToken ct)
    {
        return loginService.LoginAsync(globalArgs, loginArgs, ct);
    }
}
