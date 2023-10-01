using Flurl.Http;
using Flurl.Http.Content;
using GithubBackup.Core.Flurl;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Utils;
using Microsoft.Net.Http.Headers;
using Polly;
using Polly.Retry;

namespace GithubBackup.Core.Github.Flurl;

public static class GithubFlurlExtensions
{
    private const string RemainingRateLimitHeader = "x-ratelimit-remaining";
    private const string RateLimitResetHeader = "x-ratelimit-reset";
    private const string UserAgent = "github-backup";
    private const string Accept = "application/vnd.github.v3+json";
    private const string BaseUrl = "https://github.com";
    private const string ApiBaseUrl = "https://api.github.com";

    private static readonly FlurlClient ApiClient = new FlurlClient(ApiBaseUrl)
        .WithHeader(HeaderNames.UserAgent, UserAgent)
        .WithHeader(HeaderNames.Accept, Accept);

    private static readonly FlurlClient WebClient = new FlurlClient(BaseUrl)
        .WithHeader(HeaderNames.UserAgent, UserAgent)
        .WithHeader(HeaderNames.Accept, Accept);

    public static Task<List<TItem>> GetJsonGithubApiPagedAsync<TReponse, TItem>(
        this string urlSegments,
        int perPage,
        Func<TReponse, List<TItem>> getItems,
        CancellationToken ct)
    {
        return ApiClient
            .Request(urlSegments)
            .WithOAuthBearerToken(GithubTokenStore.Get())
            .SetQueryParam("per_page", perPage)
            .GetPagedJsonAsync(
                getItems,
                (_, _, items) => items.Count == perPage,
                (rq, index) => rq.SetQueryParam("page", index + 1),
                (rq, cancellationToken) => SendGithubAsync(rq, HttpMethod.Get, null, cancellationToken),
                ct
            );
    }
    
    public static Task<IFlurlResponse> GetGithubApiAsync(
        this string urlSegments,
        CancellationToken? ct = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        var request = ApiClient
            .Request(urlSegments)
            .WithOAuthBearerToken(GithubTokenStore.Get());

        return SendGithubAsync(request, HttpMethod.Get, null, ct, completionOption);
    }

    public static Task<IFlurlResponse> PostJsonGithubApiAsync(
        this string urlSegments,
        object data,
        CancellationToken? ct = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        var request = ApiClient
            .Request(urlSegments)
            .WithOAuthBearerToken(GithubTokenStore.Get());
        
        return PostJsonGithubAsync(request, data, ct, completionOption);
    }

    public static Task<IFlurlResponse> PostJsonGithubWebAsync(
        this string urlSegments,
        object data,
        CancellationToken? ct = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        var request = WebClient.Request(urlSegments);
        return PostJsonGithubAsync(request, data, ct, completionOption);
    }

    private static Task<IFlurlResponse> PostJsonGithubAsync(
        this IFlurlRequest request,
        object data,
        CancellationToken? ct = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        var content = new CapturedJsonContent(request.Settings.JsonSerializer.Serialize(data));
        return SendGithubAsync(request, HttpMethod.Post, content, ct, completionOption);
    }

    private static Task<IFlurlResponse> SendGithubAsync(
        this IFlurlRequest request,
        HttpMethod verb,
        HttpContent? content = null,
        CancellationToken? ct = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        var rateLimitPolicy = CreateGithubRateLimitPolicy(ct ?? CancellationToken.None);
        return rateLimitPolicy.ExecuteAsync(() => request
            .SendAsync(verb, content, ct ?? CancellationToken.None, completionOption));
    }

    private static AsyncRetryPolicy<IFlurlResponse> CreateGithubRateLimitPolicy(CancellationToken ct)
    {
        return Policy
            .HandleResult<IFlurlResponse>(response => response.Headers.GetRequired(RemainingRateLimitHeader) == "0")
            .RetryForeverAsync(response =>
            {
                var rateLimitReset = response.Result.Headers.GetRequired(RateLimitResetHeader);
                var rateLimitResetDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(rateLimitReset));
                var now = DateTimeOffset.UtcNow;
                var delay = rateLimitResetDateTime - now;
                return Task.Delay(delay, ct);
            });
    }
}