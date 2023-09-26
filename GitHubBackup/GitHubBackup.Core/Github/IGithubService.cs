using GitHubBackup.Core.Github.Http;

namespace GitHubBackup.Core.Github;

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