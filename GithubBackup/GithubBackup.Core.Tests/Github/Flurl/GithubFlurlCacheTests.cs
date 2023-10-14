using System.Net;
using FluentAssertions;
using Flurl.Http.Testing;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Flurl;
using GithubBackup.Core.Utils;

namespace GithubBackup.Core.Tests.Github.Flurl;

public class GithubFlurlCacheTests : IDisposable
{
    private const string Token = "token";
    private const string TestId = "TEST-ID";

    public GithubFlurlCacheTests()
    {
        GithubTokenStore.Set(Token);
    }

    [Fact]
    public async Task GetGithubApiAsync_WhenCacheHitAndETagUnmodified_ReturnCachedResponse()
    {
        const string url = "https://api.github.com/test";

        var cachedResponse = new TestPageResponse(new List<TestPageItem>());
        var notModifiedResponse = string.Empty;

        using var httpTest = new HttpTest();

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithoutHeader("If-None-Match", "1")
            .RespondWithJson(cachedResponse, (int)HttpStatusCode.OK, GetHeaders(
                new KeyValuePair<string, string>("ETag", "1"),
                new KeyValuePair<string, string>(TestId, nameof(cachedResponse))
            ));

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithHeader("If-None-Match", "1")
            .RespondWith(notModifiedResponse, (int)HttpStatusCode.NotModified, GetHeaders(
                new KeyValuePair<string, string>(TestId, nameof(notModifiedResponse))
            ));

        var result1 = await "/test"
            .GetGithubApiAsync(CancellationToken.None);

        var result2 = await "/test"
            .GetGithubApiAsync(CancellationToken.None);

        result1.Headers.GetRequired(TestId).Should().Be(nameof(cachedResponse));
        result2.Headers.GetRequired(TestId).Should().Be(nameof(cachedResponse));
    }

    [Fact]
    public async Task GetGithubApiAsync_WhenCacheHitAndETagModified_ReturnMakeCall()
    {
        const string url = "https://api.github.com/test";

        var cachedResponse = new TestPageResponse(new List<TestPageItem>());
        var newResponse = new TestPageResponse(new List<TestPageItem>());

        using var httpTest = new HttpTest();

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithoutHeader("If-None-Match", "1")
            .RespondWithJson(cachedResponse, (int)HttpStatusCode.OK, GetHeaders(
                new KeyValuePair<string, string>("ETag", "1"),
                new KeyValuePair<string, string>(TestId, nameof(cachedResponse))
            ));

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithHeader("If-None-Match", "1")
            .RespondWithJson(newResponse, (int)HttpStatusCode.OK, GetHeaders(
                new KeyValuePair<string, string>(TestId, nameof(newResponse))
            ));

        var result1 = await "/test"
            .GetGithubApiAsync(CancellationToken.None);

        var result2 = await "/test"
            .GetGithubApiAsync(CancellationToken.None);

        result1.Headers.GetRequired(TestId).Should().Be(nameof(cachedResponse));
        result2.Headers.GetRequired(TestId).Should().Be(nameof(newResponse));
    }

    [Fact]
    public async Task GetGithubApiAsync_WhenCacheHitAndETagsDontMatch_ReturnMakeCall()
    {
        var response1 = new TestPageResponse(new List<TestPageItem>());
        var response2 = new TestPageResponse(new List<TestPageItem>());

        using var httpTest = new HttpTest();

        httpTest
            .RespondWithJson(response1, (int)HttpStatusCode.OK, GetHeaders(
                new KeyValuePair<string, string>("ETag", "1"),
                new KeyValuePair<string, string>(TestId, nameof(response1))
            ))
            .RespondWithJson(response2, (int)HttpStatusCode.OK, GetHeaders(
                new KeyValuePair<string, string>("ETag", "2"),
                new KeyValuePair<string, string>(TestId, nameof(response2))
            ));

        var result1 = await "/test"
            .GetGithubApiAsync(CancellationToken.None);

        var result2 = await "/test"
            .GetGithubApiAsync(CancellationToken.None);

        result1.Headers.GetRequired(TestId).Should().Be(nameof(response1));
        result2.Headers.GetRequired(TestId).Should().Be(nameof(response2));
    }

    [Fact]
    public async Task GetGithubApiAsync_WhenCacheHitAndNoETag_ReturnMakeCall()
    {
        var response1 = new TestPageResponse(new List<TestPageItem>());
        var response2 = new TestPageResponse(new List<TestPageItem>());

        using var httpTest = new HttpTest();

        httpTest
            .RespondWithJson(response1, (int)HttpStatusCode.OK, GetHeaders(new KeyValuePair<string, string>(TestId, nameof(response1))))
            .RespondWithJson(response2, (int)HttpStatusCode.OK, GetHeaders(new KeyValuePair<string, string>(TestId, nameof(response2))));

        var result1 = await "/test"
            .GetGithubApiAsync(CancellationToken.None);

        var result2 = await "/test"
            .GetGithubApiAsync(CancellationToken.None);

        result1.Headers.GetRequired(TestId).Should().Be(nameof(response1));
        result2.Headers.GetRequired(TestId).Should().Be(nameof(response2));
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

    public void Dispose()
    {
        GithubFlurl.ClearCache();
    }
}