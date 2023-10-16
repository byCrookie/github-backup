using Flurl;
using Flurl.Http;
using Flurl.Http.Content;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace GithubBackup.Core.Github.Clients;

internal class GithubWebClient : IGithubWebClient
{
    private readonly ILogger<GithubWebClient> _logger;
    
    private const string UserAgent = "github-backup";
    private const string Accept = "application/vnd.github.v3+json";
    private const string BaseUrl = "https://github.com";
    
    private readonly Lazy<IFlurlClient> _client = new(() => new FlurlClient(BaseUrl)
        .WithHeader(HeaderNames.Accept, Accept)
        .WithHeader(HeaderNames.UserAgent, UserAgent));

    public GithubWebClient(ILogger<GithubWebClient> logger)
    {
        _logger = logger;
    }
    
    public Task<IFlurlResponse> PostJsonAsync(Url url, object data, Action<IFlurlRequest>? configure = null, CancellationToken? ct = null)
    {
        var request = _client.Value.Request(url);
        _logger.LogDebug("Posting to {Url}", request.Url);
        var content = new CapturedJsonContent(request.Settings.JsonSerializer.Serialize(data));
        return request.SendAsync(HttpMethod.Post, content, ct ?? CancellationToken.None);
    }
}