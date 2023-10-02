namespace GithubBackup.Cli.Github.Credentials;

public interface ICredentialStore
{
    public Task StoreTokenAsync(string accessToken, CancellationToken ct);
    public Task<string?> LoadTokenAsync(CancellationToken ct);
}