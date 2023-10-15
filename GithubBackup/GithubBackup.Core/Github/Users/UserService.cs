using Flurl.Http;
using GithubBackup.Core.Github.Clients;

namespace GithubBackup.Core.Github.Users;

internal sealed class UserService : IUserService
{
    private readonly IGithubApiClient _githubApiClient;

    public UserService(IGithubApiClient githubApiClient)
    {
        _githubApiClient = githubApiClient;
    }
    
    public async Task<User> WhoAmIAsync(CancellationToken ct)
    {
        var response = await _githubApiClient
            .GetAsync("/user", ct: ct) 
            .ReceiveJson<UserResponse>();

        return new User(response.Login, response.Name);
    }
}