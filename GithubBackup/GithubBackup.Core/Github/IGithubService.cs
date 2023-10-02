using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Repositories;
using GithubBackup.Core.Github.Users;

namespace GithubBackup.Core.Github;

public interface IGithubService
{
    Task<User> WhoAmIAsync(CancellationToken ct);
    Task<DeviceAndUserCodes> RequestDeviceAndUserCodesAsync(CancellationToken ct);
    Task<AccessToken> PollForAccessTokenAsync(string deviceCode, int interval, CancellationToken ct);
    Task<IReadOnlyCollection<Repository>> GetRepositoriesAsync(CancellationToken ct);
    Task<Migration> StartMigrationAsync(StartMigrationOptions options, CancellationToken ct);
    Task<List<Migration>> GetMigrationsAsync(CancellationToken ct);
    Task<Migration> GetMigrationAsync(long id, CancellationToken ct);
    Task<string> DownloadMigrationAsync(DownloadMigrationOptions options, CancellationToken ct);
}