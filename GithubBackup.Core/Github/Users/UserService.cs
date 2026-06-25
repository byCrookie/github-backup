using Flurl.Http;
using GithubBackup.Core.Github.Clients;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Core.Github.Users;

internal sealed class UserService(IGithubApiClient githubApiClient, ILogger<UserService> logger)
    : IUserService
{
    public async Task<User> WhoAmIAsync(CancellationToken ct)
    {
        logger.LogDebug("Getting user information");

        var response = await githubApiClient.GetAsync("/user", ct: ct).ReceiveJson<UserResponse>();

        return new User(response.Login, response.Name);
    }
}
