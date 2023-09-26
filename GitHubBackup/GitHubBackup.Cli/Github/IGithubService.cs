using GitHubBackup.Cli.Github.Http;

namespace GitHubBackup.Cli.Github;

internal interface IGithubService
{
    
}

internal class GithubService : IGithubService
{
    private readonly HttpClient _httpClient;

    public GithubService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(GithubHttpClient.Name);
    }
}