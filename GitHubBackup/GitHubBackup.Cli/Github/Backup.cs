using GithubBackup.Cli.Github.GithubCredentials;
using GithubBackup.Cli.Options;
using GithubBackup.Cli.Utils;
using Octokit;

namespace GithubBackup.Cli.Github;

internal class Backup : IBackup
{
    private readonly GlobalArgs _globalArgs;
    private readonly GithubBackupArgs _backupArgs;
    private readonly IAppSettingsCredentialsStore _appSettingsCredentialsStore;

    private const string ClientId = "e197b2a7e36e8a0d5ea9";

    public Backup(GlobalArgs globalArgs, GithubBackupArgs backupArgs, IAppSettingsCredentialsStore appSettingsCredentialsStore)
    {
        _globalArgs = globalArgs;
        _backupArgs = backupArgs;
        _appSettingsCredentialsStore = appSettingsCredentialsStore;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var user = await LoginAsync(ct);
        Console.WriteLine($"Logged in as {user.Name} - {user.Login} ({user.Email})");
        var githubClient = GetGitHubClient();
        var repositories = await githubClient.Repository.GetAllForCurrent();
        foreach (var repository in repositories)
        {
            Console.WriteLine($"Backing up {repository.FullName}");
        }

        if (!ContinuePrompt(_globalArgs, "Do you want to continue?"))
        {
            return;
        }

        var migration = await githubClient.Migration.Migrations
            .Start(user.Name, new StartMigrationRequest(repositories.Select(r => r.FullName).ToList()));
    }

    private static GitHubClient GetGitHubClient()
    {
        return new GitHubClient(new ProductHeaderValue("github-backup"), new AppSettingsCredentialsesStore());
    }

    private async Task<User> LoginAsync(CancellationToken ct)
    {
        var githubClient = GetGitHubClient();

        try
        {
            return await githubClient.User.Get(await _appSettingsCredentialsStore.LoadUsernameAsync(ct));
        }
        catch (Exception)
        {
            await LoginAndStoreAsync(ct);
            return await githubClient.User.Get(await _appSettingsCredentialsStore.LoadUsernameAsync(ct));
        }
    }

    private async Task LoginAndStoreAsync(CancellationToken ct)
    {
        var user = GetUser();
        var token = await GetOAuthTokenAsync();
        await _appSettingsCredentialsStore.StoreUsernameAsync(user, ct);
        await _appSettingsCredentialsStore.StoreTokenAsync(token.AccessToken, ct);
    }

    private static string GetUser()
    {
        Console.WriteLine("Enter your GitHub username:");
        var username = Console.ReadLine();
        return !string.IsNullOrWhiteSpace(username) ? username : throw new Exception("Username cannot be empty");
    }

    private static async Task<OauthToken> GetOAuthTokenAsync()
    {
        var githubClient = new GitHubClient(new ProductHeaderValue("github-backup"));
        var flowRequest = new OauthDeviceFlowRequest(ClientId);
        flowRequest.Scopes.AddAll(new[] { "repo", "user" });
        var deviceFlow = await githubClient.Oauth.InitiateDeviceFlow(flowRequest);
        Console.WriteLine($"Visit {deviceFlow.VerificationUri}{Environment.NewLine}and enter {deviceFlow.UserCode}");
        return await githubClient.Oauth.CreateAccessTokenForDeviceFlow(ClientId, deviceFlow);
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