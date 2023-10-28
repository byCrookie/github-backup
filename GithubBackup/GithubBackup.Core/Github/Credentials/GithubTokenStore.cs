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

    public Task SetAsync(string? token)
    {
        _logger.LogDebug("Setting token");
        _token = token;
        return Task.CompletedTask;
    }

    public Task<string> GetAsync()
    {
        if (_token is null)
        {
            throw new InvalidOperationException("Token not set");
        }

        _logger.LogDebug("Getting token");
        return Task.FromResult(_token);
    }
}