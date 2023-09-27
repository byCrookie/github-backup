using GitHubBackup.Cli.Options;
using GitHubBackup.Core.Github;

namespace GitHubBackup.Cli.Github;

internal class GithubBackup : IGithubBackup
{
    private readonly GlobalArgs _globalArgs;
    private readonly GithubBackupArgs _backupArgs;
    private readonly IGithubService _githubService;

    public GithubBackup(GlobalArgs globalArgs, GithubBackupArgs backupArgs, IGithubService githubService)
    {
        _globalArgs = globalArgs;
        _backupArgs = backupArgs;
        _githubService = githubService;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var deviceAndUserCodes = await _githubService.RequestDeviceAndUserCodesAsync(ct);
        Console.WriteLine($"Go to {deviceAndUserCodes.VerificationUri} and enter {deviceAndUserCodes.UserCode} to authenticate.");
        Console.WriteLine($"You have {deviceAndUserCodes.ExpiresIn} seconds to authenticate before the code expires.");
        var accessToken = await _githubService.PollForAccessTokenAsync(deviceAndUserCodes.DeviceCode, deviceAndUserCodes.Interval, ct);
    }
}