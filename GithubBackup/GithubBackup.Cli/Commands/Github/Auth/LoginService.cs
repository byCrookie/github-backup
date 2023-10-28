using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Auth;

internal sealed class LoginService : ILoginService
{
    private readonly ILogger<LoginService> _logger;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly IAuthenticationService _authenticationService;
    private readonly IAnsiConsole _ansiConsole;
    private readonly IGithubTokenStore _githubTokenStore;

    public LoginService(
        ILogger<LoginService> logger,
        IUserService userService,
        IConfiguration configuration,
        IAuthenticationService authenticationService,
        IAnsiConsole ansiConsole,
        IGithubTokenStore githubTokenStore)
    {
        _logger = logger;
        _userService = userService;
        _configuration = configuration;
        _authenticationService = authenticationService;
        _ansiConsole = ansiConsole;
        _githubTokenStore = githubTokenStore;
    }

    public async Task<User> LoginAsync(
        GlobalArgs globalArgs,
        LoginArgs args,
        Func<string, CancellationToken, Task> onTokenAsync,
        CancellationToken ct
    )
    {
        if (!string.IsNullOrWhiteSpace(args.Token))
        {
            _logger.LogInformation("Using token from command line");
            await _githubTokenStore.SetAsync(args.Token);
            await onTokenAsync(args.Token, ct);
            return await _userService.WhoAmIAsync(ct);
        }

        if (args.DeviceFlowAuth)
        {
            _logger.LogInformation("Using device flow authentication");
            var oauthToken = await GetOAuthTokenAsync(globalArgs, ct);
            await _githubTokenStore.SetAsync(oauthToken);
            await onTokenAsync(oauthToken, ct);
            return await _userService.WhoAmIAsync(ct);
        }

        _logger.LogInformation("Using token from environment variable");
        var token = _configuration.GetValue<string>("TOKEN");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("No token found. Please provide a token via the --token argument or the GITHUB_BACKUP_TOKEN environment variable.");
        }

        await _githubTokenStore.SetAsync(token);
        var user = await _userService.WhoAmIAsync(ct);

        if (!globalArgs.Quiet)
        {
            _ansiConsole.WriteLine($"Logged in as {user.Name}");
        }

        return user;
    }

    private async Task<string> GetOAuthTokenAsync(GlobalArgs globalArgs, CancellationToken ct)
    {
        var deviceAndUserCodes = await _authenticationService.RequestDeviceAndUserCodesAsync(ct);

        if (!globalArgs.Quiet)
        {
            _ansiConsole.WriteLine(
                $"Go to {deviceAndUserCodes.VerificationUri}{Environment.NewLine}and enter {deviceAndUserCodes.UserCode}");
            _ansiConsole.WriteLine($"You have {deviceAndUserCodes.ExpiresIn} seconds to authenticate before the code expires.");
        }
        else
        {
            _ansiConsole.WriteLine($"{deviceAndUserCodes.VerificationUri} - {deviceAndUserCodes.UserCode}");
        }

        var accessToken = await _authenticationService.PollForAccessTokenAsync(
            deviceAndUserCodes.DeviceCode,
            deviceAndUserCodes.Interval,
            ct
        );
        return accessToken.Token;
    }
}