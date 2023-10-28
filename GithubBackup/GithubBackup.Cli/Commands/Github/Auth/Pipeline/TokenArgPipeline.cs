using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Users;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Github.Auth.Pipeline;

internal class TokenArgPipeline : ITokenArgPipeline
{
    private readonly ILogger<TokenArgPipeline> _logger;
    private readonly IGithubTokenStore _githubTokenStore;
    private readonly IPersistentCredentialStore _persistentCredentialStore;
    private readonly IUserService _userService;

    public TokenArgPipeline(
        ILogger<TokenArgPipeline> logger,
        IGithubTokenStore githubTokenStore,
        IPersistentCredentialStore persistentCredentialStore,
        IUserService userService
    )
    {
        _logger = logger;
        _githubTokenStore = githubTokenStore;
        _persistentCredentialStore = persistentCredentialStore;
        _userService = userService;
    }

    public ILoginPipeline? Next { get; set; }

    private static bool IsReponsible(LoginArgs args)
    {
        return !string.IsNullOrWhiteSpace(args.Token);
    }

    public async Task<User?> LoginAsync(GlobalArgs globalArgs, LoginArgs args, bool persist, CancellationToken ct)
    {
        if (!IsReponsible(args))
        {
            return await Next!.LoginAsync(globalArgs, args, persist, ct);
        }
        
        _logger.LogInformation("Using token from command line");

        try
        {
            await _githubTokenStore.SetAsync(args.Token);
            var user = await _userService.WhoAmIAsync(ct);
            
            if (persist)
            {
                await _persistentCredentialStore.StoreTokenAsync(args.Token!, ct);
            }

            return user;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Token {Token} is invalid", args.Token);
            throw new Exception($"Token {args.Token} is invalid");
        }
    }
}