namespace GithubBackup.Cli.Commands.Github.Auth;

internal interface IPersistentCredentialStore
{
    public Task StoreTokenAsync(string accessToken, CancellationToken ct);
    public Task<string?> LoadTokenAsync(CancellationToken ct);
}