namespace GithubBackup.Core.Github.Authentication;

public interface IAuthenticationService
{
    Task<DeviceAndUserCodes> RequestDeviceAndUserCodesAsync(CancellationToken ct);
    Task<AccessToken> PollForAccessTokenAsync(
        string deviceCode,
        int interval,
        CancellationToken ct
    );
}
