using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;

namespace GithubBackup.Cli.Commands.Github.Login;

internal sealed class LoginRunner : ILoginRunner
{
    private readonly GlobalArgs _globalArgs;
    private readonly LoginArgs _loginArgs;
    private readonly ILoginService _loginService;

    public LoginRunner(
        GlobalArgs globalArgs,
        LoginArgs loginArgs,
        ILoginService loginService)
    {
        _globalArgs = globalArgs;
        _loginArgs = loginArgs;
        _loginService = loginService;
    }

    public Task RunAsync(CancellationToken ct)
    {
        return _loginService.WithoutPersistentAsync(
            _globalArgs,
            _loginArgs,
            true,
            ct
        );
    }
}