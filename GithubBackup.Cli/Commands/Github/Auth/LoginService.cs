using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Users;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Auth;

internal sealed class LoginService : ILoginService
{
    private readonly ILogger<LoginService> _logger;
    private readonly IAnsiConsole _ansiConsole;
    private readonly IConfiguration _configuration;
    private readonly IGithubTokenStore _githubTokenStore;
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authenticationService;
    private readonly ITemporaryCredentialStore _temporaryCredentialStore;
    private readonly IDateTimeOffsetProvider _dateTimeOffsetProvider;

    public LoginService(
        ILogger<LoginService> logger,
        IAnsiConsole ansiConsole,
        IConfiguration configuration,
        IGithubTokenStore githubTokenStore,
        IUserService userService,
        IAuthenticationService authenticationService,
        ITemporaryCredentialStore temporaryCredentialStore,
        IDateTimeOffsetProvider dateTimeOffsetProvider
    )
    {
        _logger = logger;
        _ansiConsole = ansiConsole;
        _configuration = configuration;
        _githubTokenStore = githubTokenStore;
        _userService = userService;
        _authenticationService = authenticationService;
        _temporaryCredentialStore = temporaryCredentialStore;
        _dateTimeOffsetProvider = dateTimeOffsetProvider;
    }

    public async Task<User?> TryLoginWithTemporaryTokenAsync(
        GlobalArgs globalArgs,
        LoginArgs args,
        CancellationToken ct
    )
    {
        var user = await LoginWithTemporaryTokenAsync(args, ct);

        if (!globalArgs.Quiet && user is not null)
        {
            _ansiConsole.WriteLine($"Logged in as {user.Name}");
            _logger.LogInformation("Logged in as {Username}", user.Name);
        }

        return user;
    }

    public async Task<User> LoginAsync(GlobalArgs globalArgs, LoginArgs args, CancellationToken ct)
    {
        var user = await LoginWithTokenArgumentAsync(args, ct)
            ?? await LoginWithEnvironmentTokenAsync(ct)
            ?? await LoginWithTemporaryTokenAsync(args, ct)
            ?? await LoginWithDeviceFlowAsync(globalArgs, ct);

        if (user is null)
        {
            throw new Exception("Login failed");
        }

        if (!globalArgs.Quiet)
        {
            _ansiConsole.WriteLine($"Logged in as {user.Name}");
            _logger.LogInformation("Logged in as {Username}", user.Name);
        }

        return user;
    }

    private async Task<User?> LoginWithTokenArgumentAsync(LoginArgs args, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(args.Token))
        {
            return null;
        }

        _logger.LogInformation("Using token from command line");
        return await ValidateTokenAsync(args.Token, ct);
    }

    private async Task<User?> LoginWithEnvironmentTokenAsync(CancellationToken ct)
    {
        var token = _configuration.GetValue<string>("TOKEN");

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        _logger.LogInformation("Using token from environment variable");
        return await ValidateTokenAsync(token, ct);
    }

    private async Task<User?> LoginWithTemporaryTokenAsync(LoginArgs args, CancellationToken ct)
    {
        if (args.DeviceFlowAuth)
        {
            return null;
        }

        _logger.LogInformation("Using temporary token cache");
        var credential = await _temporaryCredentialStore.LoadTokenAsync(ct);

        if (credential is null || string.IsNullOrWhiteSpace(credential.Token))
        {
            _logger.LogInformation("Temporary token not found");
            return null;
        }

        if (credential.ExpiresAt is not null && credential.ExpiresAt <= _dateTimeOffsetProvider.UtcNow)
        {
            _logger.LogInformation("Temporary token has expired");
            await _temporaryCredentialStore.DeleteTokenAsync(ct);
            return null;
        }

        try
        {
            return await ValidateTokenAsync(credential.Token, ct);
        }
        catch (Exception e)
        {
            _logger.LogInformation("Temporary token is invalid: {Exception}", e.Message);
            await _temporaryCredentialStore.DeleteTokenAsync(ct);
            return null;
        }
    }

    private async Task<User?> LoginWithDeviceFlowAsync(GlobalArgs globalArgs, CancellationToken ct)
    {
        _logger.LogInformation("Using device flow authentication");
        var accessToken = await GetOAuthTokenAsync(globalArgs, ct);
        var user = await ValidateTokenAsync(accessToken.Token, ct);
        var expiresAt = accessToken.ExpiresIn is null
            ? (DateTimeOffset?)null
            : _dateTimeOffsetProvider.UtcNow.AddSeconds(accessToken.ExpiresIn.Value);

        await _temporaryCredentialStore.StoreTokenAsync(accessToken.Token, expiresAt, ct);
        return user;
    }

    private async Task<User> ValidateTokenAsync(string token, CancellationToken ct)
    {
        try
        {
            await _githubTokenStore.SetAsync(token);
            return await _userService.WhoAmIAsync(ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Token is invalid");
            throw new Exception("Token is invalid");
        }
    }

    private async Task<AccessToken> GetOAuthTokenAsync(GlobalArgs globalArgs, CancellationToken ct)
    {
        var deviceAndUserCodes = await _authenticationService.RequestDeviceAndUserCodesAsync(ct);

        if (!globalArgs.Quiet)
        {
            _ansiConsole.WriteLine(
                $"Go to {deviceAndUserCodes.VerificationUri}{Environment.NewLine}and enter {deviceAndUserCodes.UserCode}"
            );
            _ansiConsole.WriteLine(
                $"You have {deviceAndUserCodes.ExpiresIn} seconds to authenticate before the code expires."
            );
        }
        else
        {
            _ansiConsole.WriteLine(
                $"{deviceAndUserCodes.VerificationUri} - {deviceAndUserCodes.UserCode}"
            );
        }

        return await _authenticationService.PollForAccessTokenAsync(
            deviceAndUserCodes.DeviceCode,
            deviceAndUserCodes.Interval,
            ct
        );
    }
}
