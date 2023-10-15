namespace GithubBackup.Core.Github.Credentials;

public interface IGithubTokenStore
{
    void Set(string? token);
    string Get();
}