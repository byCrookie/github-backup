using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Users;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Github.Auth.Pipeline;

internal class PersistedPipeline : IPersistedPipeline
{
    private readonly ILogger<PersistedPipeline> _logger;
    private readonly IPersistentCredentialStore _persistentCredentialStore;
    private readonly IGithubTokenStore _githubTokenStore;
    private readonly IUserService _userService;

    public PersistedPipeline(
        ILogger<PersistedPipeline> logger,
        IPersistentCredentialStore persistentCredentialStore,
        IGithubTokenStore githubTokenStore,
        IUserService userService
    )
    {
        _logger = logger;
        _persistentCredentialStore = persistentCredentialStore;
        _githubTokenStore = githubTokenStore;
        _userService = userService;
    }

    public ILoginPipeline? Next { get; set; }

    private static bool IsReponsible(LoginArgs args)
    {
        return !args.DeviceFlowAuth && string.IsNullOrWhiteSpace(args.Token);
    }

    public async Task<User?> LoginAsync(GlobalArgs globalArgs, LoginArgs args, bool persist, CancellationToken ct)
    {
        if (!IsReponsible(args))
        {
            return await Next!.LoginAsync(globalArgs, args, persist, ct);
        }

        var user = await LoginUsingStoreTokenAsync(ct);

        if (user is null)
        {
            return await Next!.LoginAsync(globalArgs, args, persist, ct);
        }

        return user;
    }

    private async Task<User?> LoginUsingStoreTokenAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Using token from persistent store");
            var t = await _persistentCredentialStore.LoadTokenAsync(ct);

            if (string.IsNullOrWhiteSpace(t))
            {
                _logger.LogInformation("Persistent token not found");
                return null;
            }
            
            await _githubTokenStore.SetAsync(t);
            var user = await _userService.WhoAmIAsync(ct);
            return user;
        }
        catch (Exception e)
        {
            _logger.LogInformation("Persistent token is invalid: {Exception}", e.Message);
            return null;
        }
    }
}