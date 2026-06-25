using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Output;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Users;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Github.Auth;

internal sealed class LoginService(
    ILogger<LoginService> logger,
    ICliOutput output,
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

        if (user is not null)
        {
            output.Status($"Logged in as {user.Name}");
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
            throw new Exception("Authentication failed.");
        }

        output.Status($"Logged in as {user.Name}");
        logger.LogInformation("Logged in as {Username}", user.Name);

        return user;
    }

    private async Task<User?> LoginWithTokenArgumentAsync(LoginArgs args, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(args.Token))
        {
            return null;
        }

        logger.LogInformation("Using token from --token");
        return await ValidateTokenAsync(args.Token, ct);
    }

    private async Task<User?> LoginWithEnvironmentTokenAsync(CancellationToken ct)
    {
        var token = configuration.GetValue<string>("TOKEN");

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        logger.LogInformation("Using token from GITHUB_BACKUP_TOKEN");
        return await ValidateTokenAsync(token, ct);
    }

    private async Task<User?> LoginWithTemporaryTokenAsync(LoginArgs args, CancellationToken ct)
    {
        if (args.DeviceFlowAuth)
        {
            return null;
        }

        logger.LogDebug("Checking temporary token cache");
        var credential = await temporaryCredentialStore.LoadTokenAsync(ct);

        if (credential is null || string.IsNullOrWhiteSpace(credential.Token))
        {
            logger.LogDebug("No temporary token found");
            return null;
        }

        if (
            credential.ExpiresAt is not null
            && credential.ExpiresAt <= dateTimeOffsetProvider.UtcNow
        )
        {
            logger.LogInformation("Temporary token expired; deleting cache entry");
            await temporaryCredentialStore.DeleteTokenAsync(ct);
            return null;
        }

        try
        {
            return await ValidateTokenAsync(credential.Token, ct);
        }
        catch (Exception e)
        {
            logger.LogInformation("Temporary token is invalid; deleting cache entry: {Exception}", e.Message);
            await temporaryCredentialStore.DeleteTokenAsync(ct);
            return null;
        }
    }

    private async Task<User?> LoginWithDeviceFlowAsync(GlobalArgs globalArgs, CancellationToken ct)
    {
        logger.LogInformation("Starting GitHub device flow authentication");
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
            logger.LogError(e, "GitHub token validation failed");
            throw new Exception("GitHub token is invalid.");
        }
    }

    private async Task<AccessToken> GetOAuthTokenAsync(GlobalArgs globalArgs, CancellationToken ct)
    {
        var deviceAndUserCodes = await authenticationService.RequestDeviceAndUserCodesAsync(ct);

        if (!globalArgs.Quiet)
        {
            output.Status(
                $"Go to {deviceAndUserCodes.VerificationUri}{Environment.NewLine}and enter {deviceAndUserCodes.UserCode}"
            );
            output.Status(
                $"You have {deviceAndUserCodes.ExpiresIn} seconds to authenticate before the code expires."
            );
        }
        else
        {
            output.Error($"{deviceAndUserCodes.VerificationUri} - {deviceAndUserCodes.UserCode}");
        }

        return await authenticationService.PollForAccessTokenAsync(
            deviceAndUserCodes.DeviceCode,
            deviceAndUserCodes.Interval,
            ct
        );
    }
}
