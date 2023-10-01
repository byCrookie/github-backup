namespace GithubBackup.Core.Github;

public interface IGithubService
{
    Task<User> WhoAmIAsync(string accessToken, CancellationToken ct);
    Task<DeviceAndUserCodes> RequestDeviceAndUserCodesAsync(CancellationToken ct);
    Task<AccessToken> PollForAccessTokenAsync(string deviceCode, int interval, CancellationToken ct);
    Task<IReadOnlyCollection<Repository>> GetRepositoriesAsync(string accessToken, CancellationToken ct);
    Task<Migration> StartMigrationAsync(string accessToken, IReadOnlyCollection<string> repositories, CancellationToken ct);
}