using GithubBackup.Cli.Options;
using GithubBackup.Core.Github;
using GithubBackup.Core.TokenStorage;

namespace GithubBackup.Cli.Github;

internal class Backup : IBackup
{
    private readonly GlobalArgs _globalArgs;
    private readonly GithubBackupArgs _backupArgs;
    private readonly IGithubService _githubService;
    private readonly ITokenStorageService _tokenStorageService;

    public Backup(
        GlobalArgs globalArgs,
        GithubBackupArgs backupArgs,
        IGithubService githubService,
        ITokenStorageService tokenStorageService)
    {
        _globalArgs = globalArgs;
        _backupArgs = backupArgs;
        _githubService = githubService;
        _tokenStorageService = tokenStorageService;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var accessToken = await LoginAsync(ct);
        Console.WriteLine($"Access token: {accessToken}");
        var user = await _githubService.WhoAmIAsync(accessToken, ct);
        Console.WriteLine($"Hello, {user.Name}!");
    }

    private async Task<string> LoginAsync(CancellationToken ct)
    {
        var storedToken = await _tokenStorageService.LoadTokenAsync(ct);
        
        if (storedToken is not null)
        {
            return storedToken;
        }
        
        var deviceAndUserCodes = await _githubService.RequestDeviceAndUserCodesAsync(ct);
        Console.WriteLine($"Go to {deviceAndUserCodes.VerificationUri} and enter {deviceAndUserCodes.UserCode} to authenticate.");
        Console.WriteLine($"You have {deviceAndUserCodes.ExpiresIn} seconds to authenticate before the code expires.");
        var accessToken = await _githubService.PollForAccessTokenAsync(deviceAndUserCodes.DeviceCode, deviceAndUserCodes.Interval, ct);
        Console.WriteLine($"Access token: {accessToken.Token}");
        await _tokenStorageService.StoreTokenAsync(accessToken.Token, ct);
        Console.WriteLine("Access token stored.");
        return accessToken.Token;
    }
}