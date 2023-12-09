using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Users;

namespace GithubBackup.Cli.Commands.Github.Auth;

internal interface ILoginService
{
    public Task<User?> PersistentOnlyAsync(GlobalArgs globalArgs, LoginArgs args, CancellationToken ct);
    public Task<User> WithPersistentAsync(GlobalArgs globalArgs, LoginArgs args, bool persist, CancellationToken ct);
    public Task<User> WithoutPersistentAsync(GlobalArgs globalArgs, LoginArgs args, bool persist, CancellationToken ct);
}