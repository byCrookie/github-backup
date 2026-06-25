using Microsoft.Extensions.Logging;

namespace GithubBackup.Core.Github.Credentials;

public class GithubTokenStore(ILogger<GithubTokenStore> logger) : IGithubTokenStore
{
    private string? _token;

    public Task SetAsync(string? token)
    {
        logger.LogDebug("Setting token");
        _token = token;
        return Task.CompletedTask;
    }

    public Task<string> GetAsync()
    {
        if (_token is null)
        {
            throw new InvalidOperationException("Token not set");
        }

        logger.LogDebug("Getting token");
        return Task.FromResult(_token);
    }
}
