namespace GithubBackup.Core.Github.Credentials;

public interface IGithubTokenStore
{
    Task SetAsync(string? token);
    Task<string> GetAsync();
}
