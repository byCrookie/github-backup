namespace GithubBackup.Core.Github;

public interface IGithubService
{
    Task<DeviceAndUserCodesResponse> RequestDeviceAndUserCodesAsync(CancellationToken ct);
    Task<AccessTokenResponse> PollForAccessTokenAsync(string deviceCode, int interval, CancellationToken ct);
}