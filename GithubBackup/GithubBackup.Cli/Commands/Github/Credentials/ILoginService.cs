using GithubBackup.Core.Github.Users;

namespace GithubBackup.Cli.Commands.Github.Credentials;

internal interface ILoginService
{
    public Task<User> ValidateLoginAsync(CancellationToken ct);
}