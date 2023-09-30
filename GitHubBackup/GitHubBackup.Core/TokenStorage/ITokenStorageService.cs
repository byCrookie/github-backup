namespace GithubBackup.Core.TokenStorage;

public interface ITokenStorageService
{
    public Task StoreTokenAsync(string accessToken, CancellationToken ct);
    public Task<string?> LoadTokenAsync(CancellationToken ct);
}