using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Login;

internal sealed class LoginRunner : ILoginRunner
{
    private readonly GlobalArgs _globalArgs;
    private readonly LoginArgs _loginArgs;
    private readonly IAuthenticationService _authenticationService;
    private readonly ICredentialStore _credentialStore;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginRunner> _logger;
    private readonly IAnsiConsole _ansiConsole;

    public LoginRunner(
        GlobalArgs globalArgs,
        LoginArgs loginArgs,
        IAuthenticationService authenticationService,
        ICredentialStore credentialStore,
        IUserService userService,
        IConfiguration configuration,
        ILogger<LoginRunner> logger,
        IAnsiConsole ansiConsole)
    {
        _globalArgs = globalArgs;
        _loginArgs = loginArgs;
        _authenticationService = authenticationService;
        _credentialStore = credentialStore;
        _userService = userService;
        _configuration = configuration;
        _logger = logger;
        _ansiConsole = ansiConsole;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var user = await LoginAsync(ct);

        if (!_globalArgs.Quiet)
        {
            _ansiConsole.WriteLine($"Logged in as {user.Name}");
        }
    }

    private async Task<User> LoginAsync(CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(_loginArgs.Token))
        {
            _logger.LogInformation("Using token from command line");
            await _credentialStore.StoreTokenAsync(_loginArgs.Token, ct);
            return await _userService.WhoAmIAsync(ct);
        }

        if (_loginArgs.DeviceFlowAuth)
        {
            _logger.LogInformation("Using device flow authentication");
            var oauthToken = await GetOAuthTokenAsync(ct);
            await _credentialStore.StoreTokenAsync(oauthToken, ct);
            return await _userService.WhoAmIAsync(ct);
        }

        _logger.LogInformation("Using token from environment variable");
        var token = _configuration.GetValue<string>("TOKEN");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("No token found. Please provide a token via the --token argument or the GITHUB_BACKUP_TOKEN environment variable.");
        }

        return await _userService.WhoAmIAsync(ct);
    }

    private async Task<string> GetOAuthTokenAsync(CancellationToken ct)
    {
        var deviceAndUserCodes = await _authenticationService.RequestDeviceAndUserCodesAsync(ct);
        _ansiConsole.WriteLine(
            $"Go to {deviceAndUserCodes.VerificationUri}{Environment.NewLine}and enter {deviceAndUserCodes.UserCode}");
        _ansiConsole.WriteLine($"You have {deviceAndUserCodes.ExpiresIn} seconds to authenticate before the code expires.");
        var accessToken = await _authenticationService.PollForAccessTokenAsync(
            deviceAndUserCodes.DeviceCode,
            deviceAndUserCodes.Interval,
            ct
        );
        return accessToken.Token;
    }
}