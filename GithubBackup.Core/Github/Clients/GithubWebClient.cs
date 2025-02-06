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
    
    public async Task<IFlurlResponse> PostJsonAsync(Url url, object data, Action<IFlurlRequest>? configure = null, CancellationToken? ct = null)
    {
        var request = _client.Value.Request(url);
        _logger.LogDebug("Posting to {Url}", request.Url);
        var json = request.Settings.JsonSerializer.Serialize(data);
        _logger.LogTrace("Sending {Verb} request to {Url} with content {Content}", HttpMethod.Post, request.Url, json);
        var content = new CapturedJsonContent(json);
        var response = await request.SendAsync(HttpMethod.Post, content, cancellationToken: ct ?? CancellationToken.None);
        _logger.LogTrace("Received {StatusCode} response from {Url} with content {Content}", response.StatusCode, request.Url, await response.GetStringAsync());
        return response;
    }
}