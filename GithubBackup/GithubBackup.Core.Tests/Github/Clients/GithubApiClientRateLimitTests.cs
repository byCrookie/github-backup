using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Flurl.Http;
using Flurl.Http.Testing;
using GithubBackup.Core.Github.Clients;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Tests.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace GithubBackup.Core.Tests.Github.Clients;

public class GithubApiClientRateLimitTests
{
    private readonly GithubApiClient _sut;

    private const string Token = "token";

    public GithubApiClientRateLimitTests()
    {
        var store = new GithubTokenStore();
        store.Set(Token);
        _sut = new GithubApiClient(new MemoryCache(new MemoryCacheOptions()), store);
    }

    [Fact]
    public async Task GetGithubApiAsync_WhenHitSecondaryRateLimits_ReturnAfterRetry()
    {
        var response = new TestPageResponse(new List<TestPageItem>());

        using var httpTest = new HttpTest();

        httpTest
            .RespondWith(string.Empty, (int)HttpStatusCode.TooManyRequests, GetHeaders(new KeyValuePair<string, string>("retry-after", "1")))
            .RespondWithJson(response, (int)HttpStatusCode.OK, GetHeaders(new KeyValuePair<string, string>("ETag", "1")))
            .SimulateException(new UnreachableException());

        var result = await _sut
            .GetAsync("/test")
            .ReceiveJson<TestPageResponse>();

        result.Should().BeEquivalentTo(response);
    }

    [Fact]
    public void GetGithubApiAsync_WhenHitSecondaryRateLimits_RetryAfter1s()
    {
        var response = new TestPageResponse(new List<TestPageItem>());

        using var httpTest = new HttpTest();

        httpTest
            .RespondWith(string.Empty, (int)HttpStatusCode.TooManyRequests, GetHeaders(new KeyValuePair<string, string>("retry-after", "1")))
            .RespondWithJson(response, (int)HttpStatusCode.OK, GetHeaders(new KeyValuePair<string, string>("ETag", "1")))
            .SimulateException(new UnreachableException());

        var action = () => _sut
            .GetAsync("/test")
            .ReceiveJson<TestPageResponse>();

        action.ExecutionTime().Should().BeCloseTo(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(0.1));
    }

    [Fact]
    public async Task GetGithubApiAsync_WhenHitRateLimits_ReturnAfterResetTime()
    {
        var response = new TestPageResponse(new List<TestPageItem>());

        using var httpTest = new HttpTest();

        httpTest
            .RespondWith(string.Empty, (int)HttpStatusCode.OK, GetHeaders(0, DateTimeOffset.UtcNow.AddSeconds(1)))
            .RespondWithJson(response, (int)HttpStatusCode.OK, GetHeaders(100, DateTimeOffset.UtcNow.AddSeconds(1)))
            .SimulateException(new UnreachableException());

        var result = await _sut
            .GetAsync("/test")
            .ReceiveJson<TestPageResponse>();

        result.Should().BeEquivalentTo(response);
    }

    [Fact]
    public void GetGithubApiAsync_WhenHitRateLimits_RetryAfter1s()
    {
        var response = new TestPageResponse(new List<TestPageItem>());

        using var httpTest = new HttpTest();

        httpTest
            .RespondWith(string.Empty, (int)HttpStatusCode.OK, GetHeaders(0, DateTimeOffset.UtcNow.AddSeconds(2)))
            .RespondWithJson(response, (int)HttpStatusCode.OK, GetHeaders(100, DateTimeOffset.UtcNow.AddSeconds(1)))
            .SimulateException(new UnreachableException());

        var action = () => _sut
            .GetAsync("/test")
            .ReceiveJson<TestPageResponse>();

        action.ExecutionTime().Should().BeCloseTo(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(0.2));
    }

    [Fact]
    public async Task GetGithubApiAsync_WhenHitException_ReturnAfter3Attempts()
    {
        var response = new TestPageResponse(new List<TestPageItem>());

        using var httpTest = new HttpTest();

        httpTest
            .RespondWithJson(response, (int)HttpStatusCode.BadRequest, GetHeaders())
            .RespondWithJson(response, (int)HttpStatusCode.BadRequest, GetHeaders())
            .RespondWithJson(response, (int)HttpStatusCode.BadRequest, GetHeaders())
            .RespondWithJson(response, (int)HttpStatusCode.OK, GetHeaders())
            .SimulateException(new UnreachableException());

        var result = await _sut
            .GetAsync("/test")
            .ReceiveJson<TestPageResponse>();

        result.Should().BeEquivalentTo(response);
    }

    [Fact]
    public void GetGithubApiAsync_WhenHitException_RetriesTakeTime()
    {
        var response = new TestPageResponse(new List<TestPageItem>());

        using var httpTest = new HttpTest();

        httpTest
            .RespondWithJson(response, (int)HttpStatusCode.BadRequest, GetHeaders())
            .RespondWithJson(response, (int)HttpStatusCode.BadRequest, GetHeaders())
            .RespondWithJson(response, (int)HttpStatusCode.BadRequest, GetHeaders())
            .RespondWithJson(response, (int)HttpStatusCode.OK, GetHeaders())
            .SimulateException(new UnreachableException());

        var action = () => _sut
            .GetAsync("/test")
            .ReceiveJson<TestPageResponse>();

        action.ExecutionTime().Should().BeGreaterThan(TimeSpan.FromSeconds(3));
    }

    private static Dictionary<string, string> GetHeaders(int rateLimitRemaining, DateTimeOffset rateLimitReset)
    {
        return new Dictionary<string, string>
        {
            { "x-ratelimit-remaining", $"{rateLimitRemaining}" },
            { "x-ratelimit-reset", $"{rateLimitReset.ToUnixTimeSeconds()}" }
        };
    }

    private static Dictionary<string, string> GetHeaders(params KeyValuePair<string, string>[] headers)
    {
        var allHeaders = new Dictionary<string, string>
        {
            { "x-ratelimit-remaining", "4999" },
            { "x-ratelimit-reset", "1614556800" }
        };

        foreach (var header in headers)
        {
            allHeaders.Add(header.Key, header.Value);
        }

        return allHeaders;
    }
}