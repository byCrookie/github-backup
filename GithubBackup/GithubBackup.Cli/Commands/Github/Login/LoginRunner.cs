using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Global;

namespace GithubBackup.Cli.Commands.Github.Login;

internal sealed class LoginRunner : ILoginRunner
{
    private readonly GlobalArgs _globalArgs;
    private readonly LoginArgs _loginArgs;
    private readonly IPersistentCredentialStore _persistentCredentialStore;
    private readonly ILoginService _loginService;

    public LoginRunner(
        GlobalArgs globalArgs,
        LoginArgs loginArgs,
        IPersistentCredentialStore persistentCredentialStore,
        ILoginService loginService)
    {
        _globalArgs = globalArgs;
        _loginArgs = loginArgs;
        _persistentCredentialStore = persistentCredentialStore;
        _loginService = loginService;
    }

    public Task RunAsync(CancellationToken ct)
    {
        return _loginService.LoginAsync(
            _globalArgs,
            _loginArgs,
            (token, cancellationToken) => _persistentCredentialStore.StoreTokenAsync(token, cancellationToken),
            ct
        );
    }
}