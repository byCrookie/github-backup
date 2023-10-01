using GithubBackup.Cli.Github.Credentials;
using GithubBackup.Cli.Options;
using GithubBackup.Core.Github;

namespace GithubBackup.Cli.Github;

internal class Backup : IBackup
{
    private readonly GlobalArgs _globalArgs;
    private readonly GithubBackupArgs _backupArgs;
    private readonly IGithubService _githubService;
    private readonly ICredentialStore _credentialStore;

    public Backup(
        GlobalArgs globalArgs,
        GithubBackupArgs backupArgs,
        IGithubService githubService,
        ICredentialStore credentialStore)
    {
        _globalArgs = globalArgs;
        _backupArgs = backupArgs;
        _githubService = githubService;
        _credentialStore = credentialStore;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var user = await LoginAsync(ct);
        Console.WriteLine($"Logged in as {user.Name}");
        
        var token = await _credentialStore.LoadTokenAsync(ct);

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("No token found.");
        }
        
        var repositories = await _githubService.GetRepositoriesAsync(token, ct);

        if (!repositories.Any())
        {
            Console.WriteLine("No repositories found.");
            return;
        }
        
        Console.WriteLine("Found the following repositories:");
        foreach (var repository in repositories)
        {
            Console.WriteLine($"- {repository.FullName}");
        }

        if (!ContinuePrompt(_globalArgs, "Do you want to backup these repositories?"))
        {
            return;
        }
    }
    
    private async Task<User> LoginAsync(CancellationToken ct)
    {
        try
        {
            var loadedToken = await _credentialStore.LoadTokenAsync(ct);

            if (string.IsNullOrWhiteSpace(loadedToken))
            {
                await LoginAndStoreAsync(ct);
                var newToken = await _credentialStore.LoadTokenAsync(ct);
                return await _githubService.WhoAmIAsync(newToken!, ct);
            }
            
            return await _githubService.WhoAmIAsync(loadedToken, ct);
        }
        catch (Exception)
        {
            await LoginAndStoreAsync(ct);
            var newToken = await _credentialStore.LoadTokenAsync(ct);
            return await _githubService.WhoAmIAsync(newToken!, ct);
        }
    }

    private async Task LoginAndStoreAsync(CancellationToken ct)
    {
        var token = await GetOAuthTokenAsync(ct);
        await _credentialStore.StoreTokenAsync(token, ct);
    }

    private async Task<string> GetOAuthTokenAsync(CancellationToken ct)
    {
        var deviceAndUserCodes = await _githubService.RequestDeviceAndUserCodesAsync(ct);
        Console.WriteLine($"Go to {deviceAndUserCodes.VerificationUri}{Environment.NewLine}and enter {deviceAndUserCodes.UserCode}");
        Console.WriteLine($"You have {deviceAndUserCodes.ExpiresIn} seconds to authenticate before the code expires.");
        var accessToken = await _githubService.PollForAccessTokenAsync(deviceAndUserCodes.DeviceCode, deviceAndUserCodes.Interval, ct);
        await _credentialStore.StoreTokenAsync(accessToken.Token, ct);
        return accessToken.Token;
    }
    
    private static bool ContinuePrompt(GlobalArgs globalArgs, string message)
    {
        if (globalArgs.Interactive)
        {
            Console.WriteLine($"{message} (y/n)");
            var key = Console.ReadKey();
            Console.WriteLine();
            return key.Key == ConsoleKey.Y;
        }

        return false;
    }
}