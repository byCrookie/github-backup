namespace GithubBackup.Cli.Github.GithubCredentials;

public interface IAppSettingsCredentialsStore
{
    public Task StoreUsernameAsync(string user, CancellationToken ct);
    public Task<string?> LoadUsernameAsync(CancellationToken ct);
    public Task StoreTokenAsync(string accessToken, CancellationToken ct);
    public Task<string?> LoadTokenAsync(CancellationToken ct);
}