using System.Net;
using Flurl;
using Flurl.Http;
using Flurl.Http.Content;
using GithubBackup.Core.Flurl;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Utils;
using Microsoft.Net.Http.Headers;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace GithubBackup.Core.Github.Flurl;

public static class GithubFlurlExtensions
{
    private const string RemainingRateLimitHeader = "x-ratelimit-remaining";
    private const string RateLimitResetHeader = "x-ratelimit-reset";
    private const string RetryAfterHeader = "retry-after";
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
        this Url url,
        int perPage,
        Func<TReponse, List<TItem>> getItems,
        CancellationToken ct)
    {
        return ApiClient
            .Request(url)
            .WithOAuthBearerToken(GithubTokenStore.Get())
            .SetQueryParam("per_page", perPage)
            .GetPagedJsonAsync(
                getItems,
                (_, _, items) => items.Count == perPage,
                (rq, index) => rq.SetQueryParam("page", index + 1),
                (rq, cancellationToken) => SendGithubApiAsync(rq, HttpMethod.Get, null, cancellationToken),
                ct
            );
    }
    
    public static Task<string> DownloadFileGithubApiAsync(
        this string urlSegments,
        string localFolderPath,
        string? localFileName = null,
        int bufferSize = 4096,
        CancellationToken? ct = null)
    {
        return ApiClient
            .Request(urlSegments)
            .WithOAuthBearerToken(GithubTokenStore.Get())
            .DownloadFileAsync(localFolderPath, localFileName, bufferSize, ct ?? CancellationToken.None);
    }
    
    public static Task<IFlurlResponse> GetGithubApiAsync(
        this string urlSegments,
        CancellationToken? ct = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        var request = ApiClient
            .Request(urlSegments)
            .WithOAuthBearerToken(GithubTokenStore.Get());

        return SendGithubApiAsync(request, HttpMethod.Get, null, ct, completionOption);
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
        
        var content = new CapturedJsonContent(request.Settings.JsonSerializer.Serialize(data));
        return SendGithubApiAsync(request, HttpMethod.Post, content, ct, completionOption);
    }

    public static Task<IFlurlResponse> PostJsonGithubWebAsync(
        this string urlSegments,
        object data,
        CancellationToken? ct = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        var request = WebClient.Request(urlSegments);
        var content = new CapturedJsonContent(request.Settings.JsonSerializer.Serialize(data));
        return request.SendAsync(HttpMethod.Post, content, ct ?? CancellationToken.None, completionOption);
    }

    private static Task<IFlurlResponse> SendGithubApiAsync(
        this IFlurlRequest request,
        HttpMethod verb,
        HttpContent? content = null,
        CancellationToken? ct = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        var retryPolicy = CreateGithubRetryPolicy();
        var rateLimitPolicy = CreateGithubRateLimitPolicy(ct ?? CancellationToken.None);
        var retryAfterPolicy = CreateGithubRetryAfterPolicy(ct ?? CancellationToken.None);
        var policy = Policy.WrapAsync(retryPolicy, rateLimitPolicy, retryAfterPolicy);
        return policy.ExecuteAsync(() => request
            .SendAsync(verb, content, ct ?? CancellationToken.None, completionOption));
    }

    private static IAsyncPolicy<IFlurlResponse> CreateGithubRateLimitPolicy(CancellationToken ct)
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
    
    private static IAsyncPolicy<IFlurlResponse> CreateGithubRetryAfterPolicy(CancellationToken ct)
    {
        return Policy
            .HandleResult<IFlurlResponse>(response => !string.IsNullOrWhiteSpace(response.Headers.Get(RetryAfterHeader)))
            .RetryForeverAsync(response =>
            {
                var resetAfter = response.Result.Headers.GetRequired(RetryAfterHeader);
                var delay = TimeSpan.FromSeconds(int.Parse(resetAfter));
                return Task.Delay(delay, ct);
            });
    }
    
    private static IAsyncPolicy<IFlurlResponse> CreateGithubRetryPolicy()
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 3);

        return Policy
            .HandleResult<IFlurlResponse>(response => response.StatusCode == (int)HttpStatusCode.Forbidden)
            .WaitAndRetryAsync(delay);
    }
}