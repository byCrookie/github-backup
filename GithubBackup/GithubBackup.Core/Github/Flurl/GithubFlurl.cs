using System.Net;
using Flurl;
using Flurl.Http;
using Flurl.Http.Content;
using GithubBackup.Core.Flurl;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;

namespace GithubBackup.Core.Github.Flurl;

internal static class GithubFlurl
{
    private const string RemainingRateLimitHeader = "x-ratelimit-remaining";
    private const string RateLimitResetHeader = "x-ratelimit-reset";
    private const string RetryAfterHeader = "retry-after";
    private const string IfNoneMatchHeader = "If-None-Match";
    private const string ETagHeader = "ETag";
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

    private static IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    public static void ClearCache()
    {
        _cache.Dispose();
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public static IFlurlRequest RequestApi(this string urlSegments)
    {
        return new Url(urlSegments).RequestApi();
    }

    public static IFlurlRequest RequestWeb(this string urlSegments)
    {
        return new Url(urlSegments).RequestWeb();
    }

    public static IFlurlRequest RequestApi(this Url url)
    {
        return ApiClient.Request(url);
    }

    public static IFlurlRequest RequestWeb(this Url url)
    {
        return WebClient.Request(url);
    }

    public static Task<List<TItem>> GetJsonGithubApiPagedAsync<TReponse, TItem>(
        this IFlurlRequest request,
        int perPage,
        Func<TReponse, List<TItem>> getItems,
        CancellationToken ct)
    {
        var newRequest = request.Url.RequestApi();
        request.Url = newRequest.Url;

        return request
            .WithClient(ApiClient)
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
        return urlSegments
            .RequestApi()
            .WithOAuthBearerToken(GithubTokenStore.Get())
            .DownloadFileAsync(localFolderPath, localFileName, bufferSize, ct ?? CancellationToken.None);
    }

    public static Task<IFlurlResponse> GetGithubApiAsync(
        this string urlSegments,
        CancellationToken? ct = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        var request = urlSegments
            .RequestApi()
            .WithOAuthBearerToken(GithubTokenStore.Get());

        return SendGithubApiAsync(request, HttpMethod.Get, null, ct, completionOption);
    }

    public static Task<IFlurlResponse> PostJsonGithubApiAsync(
        this string urlSegments,
        object data,
        CancellationToken? ct = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        var request = urlSegments
            .RequestApi()
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
        var request = urlSegments.RequestWeb();
        var content = new CapturedJsonContent(request.Settings.JsonSerializer.Serialize(data));
        return request.SendAsync(HttpMethod.Post, content, ct ?? CancellationToken.None, completionOption);
    }

    private static async Task<IFlurlResponse> SendGithubApiAsync(
        this IFlurlRequest request,
        HttpMethod verb,
        HttpContent? content = null,
        CancellationToken? ct = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        var delays = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 3).ToArray();

        var resiliencePipeline = new ResiliencePipelineBuilder<IFlurlResponse>()
            .AddRetry(new RetryStrategyOptions<IFlurlResponse>
            {
                ShouldHandle = new PredicateBuilder<IFlurlResponse>()
                    .Handle<FlurlHttpException>(_ => true)
                    .HandleResult(response => !response.ResponseMessage.IsSuccessStatusCode),
                DelayGenerator = arguments => ValueTask.FromResult<TimeSpan?>(delays[arguments.AttemptNumber]),
                MaxRetryAttempts = 3
            })
            .AddRetry(new RetryStrategyOptions<IFlurlResponse>
            {
                ShouldHandle = new PredicateBuilder<IFlurlResponse>()
                    .HandleResult(response => response.Headers.GetRequired(RemainingRateLimitHeader) == "0"),
                DelayGenerator = arguments =>
                {
                    var rateLimitReset = arguments.Outcome.Result!.Headers.GetRequired(RateLimitResetHeader);
                    var rateLimitResetDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(rateLimitReset));
                    var now = DateTimeOffset.UtcNow;
                    var delay = rateLimitResetDateTime - now;
                    return ValueTask.FromResult<TimeSpan?>(delay);
                }
            })
            .AddRetry(new RetryStrategyOptions<IFlurlResponse>
            {
                ShouldHandle = new PredicateBuilder<IFlurlResponse>()
                    .Handle<FlurlHttpException>(exception => !string.IsNullOrWhiteSpace(exception.Call.Response.Headers.Get(RetryAfterHeader))),
                DelayGenerator = arguments =>
                {
                    var exception = (FlurlHttpException)arguments.Outcome.Exception!;
                    var resetAfter = exception.Call.Response.Headers.GetRequired(RetryAfterHeader);
                    return ValueTask.FromResult<TimeSpan?>(TimeSpan.FromSeconds(int.Parse(resetAfter)));
                }
            })
            .Build();

        return await resiliencePipeline.ExecuteAsync(
            async cancellationToken => await request.SendGithubApiCachedAsync(verb, content, cancellationToken, completionOption),
            ct ?? CancellationToken.None
        );
    }

    private static async Task<IFlurlResponse> SendGithubApiCachedAsync(
        this IFlurlRequest request,
        HttpMethod verb,
        HttpContent? content = null,
        CancellationToken? ct = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        var cacheKey = await GetCacheKeyAsync(request, verb, content);

        if (verb == HttpMethod.Get && _cache.TryGetValue(cacheKey, out IFlurlResponse? cachedResponse))
        {
            var modifiedResponse = await request
                .WithHeader(IfNoneMatchHeader, cachedResponse!.Headers.GetRequired(ETagHeader))
                .SendAsync(verb, content, ct ?? CancellationToken.None, completionOption);

            if (modifiedResponse.StatusCode == (int)HttpStatusCode.NotModified)
            {
                return cachedResponse;
            }
        }

        var response = await request.SendAsync(verb, content, ct ?? CancellationToken.None, completionOption);

        if (verb == HttpMethod.Get && response.StatusCode == (int)HttpStatusCode.OK && !string.IsNullOrWhiteSpace(response.Headers.Get(ETagHeader)))
        {
            _cache.Set(cacheKey, response);
        }

        return response;
    }

    private static async Task<string> GetCacheKeyAsync(IFlurlRequest request, HttpMethod verb, HttpContent? content)
    {
        var body = content is null ? string.Empty : await content.ReadAsStringAsync();
        return $"{request.Url}{verb}{body}";
    }
}