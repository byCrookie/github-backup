using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Options;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Users;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Login;

internal sealed class Login : ILogin
{
    private readonly GlobalArgs _globalArgs;
    private readonly LoginArgs _loginArgs;
    private readonly IAuthenticationService _authenticationService;
    private readonly ICredentialStore _credentialStore;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public Login(
        GlobalArgs globalArgs,
        LoginArgs loginArgs,
        IAuthenticationService authenticationService,
        ICredentialStore credentialStore,
        IUserService userService,
        IConfiguration configuration)
    {
        _globalArgs = globalArgs;
        _loginArgs = loginArgs;
        _authenticationService = authenticationService;
        _credentialStore = credentialStore;
        _userService = userService;
        _configuration = configuration;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var user = await LoginAsync(ct);

        if (!_globalArgs.Quiet)
        {
            AnsiConsole.WriteLine($"Logged in as {user.Name}");
        }
    }

    private async Task<User> LoginAsync(CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(_loginArgs.Token))
        {
            await _credentialStore.StoreTokenAsync(_loginArgs.Token, ct);
            return await _userService.WhoAmIAsync(ct);
        }

        if (_loginArgs.DeviceFlowAuth)
        {
            var oauthToken = await GetOAuthTokenAsync(ct);
            await _credentialStore.StoreTokenAsync(oauthToken, ct);
            return await _userService.WhoAmIAsync(ct);
        }

        var token = _configuration.GetValue<string>("TOKEN");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("No token found. Please provide a token via the --token argument or the GITHUB_BACKUP_TOKEN environment variable.");
        }

        await _credentialStore.StoreTokenAsync(token, ct);
        return await _userService.WhoAmIAsync(ct);
    }

    private async Task<string> GetOAuthTokenAsync(CancellationToken ct)
    {
        var deviceAndUserCodes = await _authenticationService.RequestDeviceAndUserCodesAsync(ct);
        AnsiConsole.WriteLine(
            $"Go to {deviceAndUserCodes.VerificationUri}{Environment.NewLine}and enter {deviceAndUserCodes.UserCode}");
        AnsiConsole.WriteLine($"You have {deviceAndUserCodes.ExpiresIn} seconds to authenticate before the code expires.");
        var accessToken = await _authenticationService.PollForAccessTokenAsync(
            deviceAndUserCodes.DeviceCode,
            deviceAndUserCodes.Interval,
            ct
        );
        await _credentialStore.StoreTokenAsync(accessToken.Token, ct);
        return accessToken.Token;
    }
}