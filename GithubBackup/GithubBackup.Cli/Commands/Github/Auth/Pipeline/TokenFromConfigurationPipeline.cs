using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Github.Auth.Pipeline;

internal class TokenFromConfigurationPipeline : ITokenFromConfigurationPipeline
{
    private readonly ILogger<TokenFromConfigurationPipeline> _logger;
    private readonly IConfiguration _configuration;
    private readonly IGithubTokenStore _githubTokenStore;
    private readonly IUserService _userService;

    public TokenFromConfigurationPipeline(
        ILogger<TokenFromConfigurationPipeline> logger,
        IConfiguration configuration,
        IGithubTokenStore githubTokenStore,
        IUserService userService
    )
    {
        _logger = logger;
        _configuration = configuration;
        _githubTokenStore = githubTokenStore;
        _userService = userService;
    }

    public ILoginPipeline? Next { get; set; }

    private bool IsReponsible()
    {
        var token = _configuration.GetValue<string>("TOKEN");
        return !string.IsNullOrWhiteSpace(token);
    }

    public async Task<User?> LoginAsync(GlobalArgs globalArgs, LoginArgs args, bool persist, CancellationToken ct)
    {
        if (!IsReponsible())
        {
            return await Next!.LoginAsync(globalArgs, args, persist, ct);
        }
        
        _logger.LogInformation("Using token from environment variable");
        var token = _configuration.GetValue<string>("TOKEN");

        try
        {
            var user = await _userService.WhoAmIAsync(ct);
            await _githubTokenStore.SetAsync(token);
            return user;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Token {Token} is invalid", token);
            throw new Exception($"Token {token} is invalid");
        }
    }
}