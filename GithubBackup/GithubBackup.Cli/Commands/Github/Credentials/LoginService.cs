using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Users;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Credentials;

internal sealed class LoginService : ILoginService
{
    private readonly GlobalArgs _globalArgs;
    private readonly ILogger<LoginService> _logger;
    private readonly ICredentialStore _credentialStore;
    private readonly IUserService _userService;
    private readonly IAnsiConsole _ansiConsole;

    public LoginService(
        GlobalArgs globalArgs,
        ILogger<LoginService> logger,
        ICredentialStore credentialStore,
        IUserService userService,
        IAnsiConsole ansiConsole)
    {
        _globalArgs = globalArgs;
        _logger = logger;
        _credentialStore = credentialStore;
        _userService = userService;
        _ansiConsole = ansiConsole;
    }

    public async Task<User> ValidateLoginAsync(CancellationToken ct)
    {
        var token = await _credentialStore.LoadTokenAsync(ct);

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogDebug("No token found");
            throw new Exception("Login first using the 'login' command.");
        }

        var user = await TryWhoAmIAsync(ct);

        if (!_globalArgs.Interactive)
        {
            return user;
        }

        if (_ansiConsole.Confirm($"Do you want to continue as {user.Name}?"))
        {
            return user;
        }

        throw new Exception("Login with a different user using the 'login' command.");
    }

    private async Task<User> TryWhoAmIAsync(CancellationToken ct)
    {
        try
        {
            return await _userService.WhoAmIAsync(ct);
        }
        catch (Exception)
        {
            _logger.LogDebug("Token is invalid");
            throw new Exception("Login first using the 'login' command.");
        }
    }
}