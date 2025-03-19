using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Users;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Auth.Pipeline;

internal class DeviceFlowAuthPipeline : IDeviceFlowAuthPipeline
{
    private readonly ILogger<DeviceFlowAuthPipeline> _logger;
    private readonly IGithubTokenStore _githubTokenStore;
    private readonly IPersistentCredentialStore _persistentCredentialStore;
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IAnsiConsole _ansiConsole;

    public DeviceFlowAuthPipeline(
        ILogger<DeviceFlowAuthPipeline> logger,
        IGithubTokenStore githubTokenStore,
        IPersistentCredentialStore persistentCredentialStore,
        IUserService userService,
        IAuthenticationService authenticationService,
        IAnsiConsole ansiConsole
    )
    {
        _logger = logger;
        _githubTokenStore = githubTokenStore;
        _persistentCredentialStore = persistentCredentialStore;
        _userService = userService;
        _authenticationService = authenticationService;
        _ansiConsole = ansiConsole;
    }

    public ILoginPipeline? Next { get; set; }

    private static bool IsReponsible(LoginArgs args)
    {
        return args.DeviceFlowAuth;
    }

    public async Task<User?> LoginAsync(
        GlobalArgs globalArgs,
        LoginArgs args,
        bool persist,
        CancellationToken ct
    )
    {
        if (!IsReponsible(args))
        {
            return await Next!.LoginAsync(globalArgs, args, persist, ct);
        }

        _logger.LogInformation("Using device flow authentication");
        var oauthToken = await GetOAuthTokenAsync(globalArgs, ct);

        try
        {
            await _githubTokenStore.SetAsync(oauthToken);
            var user = await _userService.WhoAmIAsync(ct);

            if (persist)
            {
                await _persistentCredentialStore.StoreTokenAsync(oauthToken, ct);
            }

            return user;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Token {Token} is invalid", args.Token);
            throw new Exception($"Token {args.Token} is invalid");
        }
    }

    private async Task<string> GetOAuthTokenAsync(GlobalArgs globalArgs, CancellationToken ct)
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

        var accessToken = await _authenticationService.PollForAccessTokenAsync(
            deviceAndUserCodes.DeviceCode,
            deviceAndUserCodes.Interval,
            ct
        );
        return accessToken.Token;
    }
}
