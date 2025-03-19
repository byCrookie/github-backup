namespace GithubBackup.Core.Github.Users;

public interface IUserService
{
    Task<User> WhoAmIAsync(CancellationToken ct);
}
