using Flurl.Http;
using GithubBackup.Core.Github.Flurl;

namespace GithubBackup.Core.Github.Users;

public class UserService : IUserService
{
    public async Task<User> WhoAmIAsync(CancellationToken ct)
    {
        var response = await "/user"
            .GetGithubApiAsync(ct)
            .ReceiveJson<UserResponse>();

        return new User(response.Login, response.Name);
    }
}