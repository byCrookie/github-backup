namespace GithubBackup.Cli.Commands.Github.Credentials;

internal interface ICredentialStore
{
    public Task StoreTokenAsync(string accessToken, CancellationToken ct);
    public Task<string?> LoadTokenAsync(CancellationToken ct);
}