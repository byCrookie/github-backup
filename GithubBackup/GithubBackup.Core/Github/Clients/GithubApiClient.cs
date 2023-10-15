﻿using System.Net;
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

namespace GithubBackup.Core.Github.Clients;

internal class GithubApiClient : IGithubApiClient
{
    private readonly IMemoryCache _memoryCache;
    private readonly IGithubTokenStore _githubTokenStore;

    private const string RemainingRateLimitHeader = "x-ratelimit-remaining";
    private const string RateLimitResetHeader = "x-ratelimit-reset";
    private const string RetryAfterHeader = "retry-after";
    private const string IfNoneMatchHeader = "If-None-Match";
    private const string ETagHeader = "ETag";
    private const string UserAgent = "github-backup";
    private const string Accept = "application/vnd.github.v3+json";
    private const string BaseUrl = "https://api.github.com";

    private readonly Lazy<IFlurlClient> _client = new(() => new FlurlClient(BaseUrl)
        .WithHeader(HeaderNames.Accept, Accept)
        .WithHeader(HeaderNames.UserAgent, UserAgent));

    public GithubApiClient(IMemoryCache memoryCache, IGithubTokenStore githubTokenStore)
    {
        _memoryCache = memoryCache;
        _githubTokenStore = githubTokenStore;
    }

    public Task<List<TItem>> ReceiveJsonPagedAsync<TReponse, TItem>(Url url, int perPage, Func<TReponse, List<TItem>> getItems, Action<IFlurlRequest>? configure = null, CancellationToken? ct = null)
    {
        var request = _client.Value.Request(url);
        configure?.Invoke(request);
        return request
            .WithOAuthBearerToken(_githubTokenStore.Get())
            .SetQueryParam("per_page", perPage)
            .GetPagedJsonAsync(
                getItems,
                (_, _, items) => items.Count == perPage,
                (rq, index) => rq.SetQueryParam("page", index + 1),
                (rq, cancellationToken) => SendAsync(rq, HttpMethod.Get, null, cancellationToken),
                ct ?? CancellationToken.None
            );
    }

    public Task<string> DownloadFileAsync(Url url, string path, string? fileName = null, Action<IFlurlRequest>? configure = null, CancellationToken? ct = null)
    {
        var request = _client.Value.Request(url)
            .WithOAuthBearerToken(_githubTokenStore.Get());
        configure?.Invoke(request);
        return request.DownloadFileAsync(path, fileName, 4096, ct ?? CancellationToken.None);
    }

    public Task<IFlurlResponse> GetAsync(Url url, Action<IFlurlRequest>? configure = null, CancellationToken? ct = null)
    {
        var request = _client.Value.Request(url)
            .WithOAuthBearerToken(_githubTokenStore.Get());
        configure?.Invoke(request);
        return SendAsync(request, HttpMethod.Get, null, ct ?? CancellationToken.None);
    }

    public Task<IFlurlResponse> PostJsonAsync(Url url, object data, Action<IFlurlRequest>? configure = null, CancellationToken? ct = null)
    {
        var request = _client.Value.Request(url)
            .WithOAuthBearerToken(_githubTokenStore.Get());
        configure?.Invoke(request);
        var content = new CapturedJsonContent(request.Settings.JsonSerializer.Serialize(data));
        return SendAsync(request, HttpMethod.Post, content, ct ?? CancellationToken.None);
    }

    private async Task<IFlurlResponse> SendAsync(IFlurlRequest request, HttpMethod verb,
        HttpContent? content = null, CancellationToken? ct = null)
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
            async cancellationToken => await SendGithubApiCachedAsync(request, verb, content, cancellationToken),
            ct ?? CancellationToken.None
        );
    }

    private async Task<IFlurlResponse> SendGithubApiCachedAsync(IFlurlRequest request, HttpMethod verb,
        HttpContent? content = null, CancellationToken? ct = null)
    {
        var cacheKey = await GetCacheKeyAsync(request, verb, content);

        if (verb == HttpMethod.Get && _memoryCache.TryGetValue(cacheKey, out IFlurlResponse? cachedResponse))
        {
            var modifiedResponse = await request
                .WithHeader(IfNoneMatchHeader, cachedResponse!.Headers.GetRequired(ETagHeader))
                .SendAsync(verb, content, ct ?? CancellationToken.None, HttpCompletionOption.ResponseHeadersRead);

            if (modifiedResponse.StatusCode == (int)HttpStatusCode.NotModified)
            {
                return cachedResponse;
            }
        }

        var response = await request.SendAsync(verb, content, ct ?? CancellationToken.None, HttpCompletionOption.ResponseHeadersRead);

        if (verb == HttpMethod.Get && response.StatusCode == (int)HttpStatusCode.OK && !string.IsNullOrWhiteSpace(response.Headers.Get(ETagHeader)))
        {
            _memoryCache.Set(cacheKey, response);
        }

        return response;
    }

    private static async Task<string> GetCacheKeyAsync(IFlurlRequest request, HttpMethod verb, HttpContent? content)
    {
        var body = content is null ? string.Empty : await content.ReadAsStringAsync();
        return $"{request.Url}{verb}{body}";
    }
}