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

internal sealed class LoginService(
    ILogger<LoginService> logger,
    IAnsiConsole ansiConsole,
    IConfiguration configuration,
    IGithubTokenStore githubTokenStore,
    IUserService userService,
    IAuthenticationService authenticationService,
    ITemporaryCredentialStore temporaryCredentialStore,
    IDateTimeOffsetProvider dateTimeOffsetProvider
) : ILoginService
{
    public async Task<User?> TryLoginWithTemporaryTokenAsync(
        GlobalArgs globalArgs,
        LoginArgs args,
        CancellationToken ct
    )
    {
        var user = await LoginWithTemporaryTokenAsync(args, ct);

        if (!globalArgs.Quiet && user is not null)
        {
            ansiConsole.WriteLine($"Logged in as {user.Name}");
            logger.LogInformation("Logged in as {Username}", user.Name);
        }

        return user;
    }

    public async Task<User> LoginAsync(GlobalArgs globalArgs, LoginArgs args, CancellationToken ct)
    {
        var user =
            await LoginWithTokenArgumentAsync(args, ct)
            ?? await LoginWithEnvironmentTokenAsync(ct)
            ?? await LoginWithTemporaryTokenAsync(args, ct)
            ?? await LoginWithDeviceFlowAsync(globalArgs, ct);

        if (user is null)
        {
            throw new Exception("Login failed");
        }

        if (!globalArgs.Quiet)
        {
            ansiConsole.WriteLine($"Logged in as {user.Name}");
            logger.LogInformation("Logged in as {Username}", user.Name);
        }

        return user;
    }

    private async Task<User?> LoginWithTokenArgumentAsync(LoginArgs args, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(args.Token))
        {
            return null;
        }

        logger.LogInformation("Using token from command line");
        return await ValidateTokenAsync(args.Token, ct);
    }

    private async Task<User?> LoginWithEnvironmentTokenAsync(CancellationToken ct)
    {
        var token = configuration.GetValue<string>("TOKEN");

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        logger.LogInformation("Using token from environment variable");
        return await ValidateTokenAsync(token, ct);
    }

    private async Task<User?> LoginWithTemporaryTokenAsync(LoginArgs args, CancellationToken ct)
    {
        if (args.DeviceFlowAuth)
        {
            return null;
        }

        logger.LogInformation("Using temporary token cache");
        var credential = await temporaryCredentialStore.LoadTokenAsync(ct);

        if (credential is null || string.IsNullOrWhiteSpace(credential.Token))
        {
            logger.LogInformation("Temporary token not found");
            return null;
        }

        if (
            credential.ExpiresAt is not null
            && credential.ExpiresAt <= dateTimeOffsetProvider.UtcNow
        )
        {
            logger.LogInformation("Temporary token has expired");
            await temporaryCredentialStore.DeleteTokenAsync(ct);
            return null;
        }

        try
        {
            return await ValidateTokenAsync(credential.Token, ct);
        }
        catch (Exception e)
        {
            logger.LogInformation("Temporary token is invalid: {Exception}", e.Message);
            await temporaryCredentialStore.DeleteTokenAsync(ct);
            return null;
        }
    }

    private async Task<User?> LoginWithDeviceFlowAsync(GlobalArgs globalArgs, CancellationToken ct)
    {
        logger.LogInformation("Using device flow authentication");
        var accessToken = await GetOAuthTokenAsync(globalArgs, ct);
        var user = await ValidateTokenAsync(accessToken.Token, ct);
        var expiresAt = accessToken.ExpiresIn is null
            ? (DateTimeOffset?)null
            : dateTimeOffsetProvider.UtcNow.AddSeconds(accessToken.ExpiresIn.Value);

        await temporaryCredentialStore.StoreTokenAsync(accessToken.Token, expiresAt, ct);
        return user;
    }

    private async Task<User> ValidateTokenAsync(string token, CancellationToken ct)
    {
        try
        {
            await githubTokenStore.SetAsync(token);
            return await userService.WhoAmIAsync(ct);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Token is invalid");
            throw new Exception("Token is invalid");
        }
    }

    private async Task<AccessToken> GetOAuthTokenAsync(GlobalArgs globalArgs, CancellationToken ct)
    {
        var deviceAndUserCodes = await authenticationService.RequestDeviceAndUserCodesAsync(ct);

        if (!globalArgs.Quiet)
        {
            ansiConsole.WriteLine(
                $"Go to {deviceAndUserCodes.VerificationUri}{Environment.NewLine}and enter {deviceAndUserCodes.UserCode}"
            );
            ansiConsole.WriteLine(
                $"You have {deviceAndUserCodes.ExpiresIn} seconds to authenticate before the code expires."
            );
        }
        else
        {
            ansiConsole.WriteLine(
                $"{deviceAndUserCodes.VerificationUri} - {deviceAndUserCodes.UserCode}"
            );
        }

        return await authenticationService.PollForAccessTokenAsync(
            deviceAndUserCodes.DeviceCode,
            deviceAndUserCodes.Interval,
            ct
        );
    }
}
