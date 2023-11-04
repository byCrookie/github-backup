using Flurl.Http;
using GithubBackup.Core.Github.Clients;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Core.Github.Users;

internal sealed class UserService : IUserService
{
    private readonly IGithubApiClient _githubApiClient;
    private readonly ILogger<UserService> _logger;

    public UserService(IGithubApiClient githubApiClient, ILogger<UserService> logger)
    {
        _githubApiClient = githubApiClient;
        _logger = logger;
    }
    
    public async Task<User> WhoAmIAsync(CancellationToken ct)
    {
        _logger.LogDebug("Getting user information");
        
        var response = await _githubApiClient
            .GetAsync("/user", ct: ct) 
            .ReceiveJson<UserResponse>();

        return new User(response.Login, response.Name);
    }
}