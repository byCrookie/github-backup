using GithubBackup.Core.Github.Users;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Github.Credentials;

internal sealed class LoginService : ILoginService
{
    private readonly ILogger<LoginService> _logger;
    private readonly ICredentialStore _credentialStore;
    private readonly IUserService _userService;

    public LoginService(
        ILogger<LoginService> logger,
        ICredentialStore credentialStore,
        IUserService userService)
    {
        _logger = logger;
        _credentialStore = credentialStore;
        _userService = userService;
    }

    public async Task<User> ValidateLoginAsync(CancellationToken ct)
    {
        var token = await _credentialStore.LoadTokenAsync(ct);

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogDebug("No token found");
            throw new Exception("Login first using the 'login' command.");
        }

        return await TryWhoAmIAsync(ct);
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