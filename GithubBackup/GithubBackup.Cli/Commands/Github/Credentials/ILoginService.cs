using GithubBackup.Core.Github.Users;

namespace GithubBackup.Cli.Commands.Github.Credentials;

public interface ILoginService
{
    public Task<User> ValidateLoginAsync(CancellationToken ct);
}