using Microsoft.Extensions.Logging;

namespace GithubBackup.Core.Github.Credentials;

public class GithubTokenStore : IGithubTokenStore
{
    private readonly ILogger<GithubTokenStore> _logger;
    
    private string? _token;

    public GithubTokenStore(ILogger<GithubTokenStore> logger)
    {
        _logger = logger;
    }
    
    public void Set(string? token)
    {
        _logger.LogDebug("Setting token");
        _token = token;
    }
    
    public string Get()
    {
        if (_token is null)
        {
            throw new InvalidOperationException("Token not set");
        }
        
        _logger.LogDebug("Getting token");
        return _token;
    }
}