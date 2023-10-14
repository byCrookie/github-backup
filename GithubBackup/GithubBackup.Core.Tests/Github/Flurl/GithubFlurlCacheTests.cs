using System.Net;
using FluentAssertions;
using Flurl.Http;
using Flurl.Http.Testing;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Flurl;

namespace GithubBackup.Core.Tests.Github.Flurl;

public class GithubFlurlCacheTests : IDisposable
{
    private const string Token = "token";

    public GithubFlurlCacheTests()
    {
        GithubTokenStore.Set(Token);
    }

    [Fact]
    public async Task GetGithubApiAsync_WhenCacheHitAndETagUnmodified_ReturnCachedResponse()
    {
        const string url = "https://api.github.com/test";

        var cachedResponse = new TestPageResponse(new List<TestPageItem>());
        
        using var httpTest = new HttpTest();

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithoutHeader("If-None-Match", "1")
            .RespondWithJson(cachedResponse, (int)HttpStatusCode.OK, GetHeaders(new KeyValuePair<string, string>("ETag", "1")));
        
        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithHeader("If-None-Match", "1")
            .RespondWith(string.Empty, (int)HttpStatusCode.NotModified, GetHeaders());

        var result1 = await "/test"
            .GetGithubApiAsync(CancellationToken.None)
            .ReceiveJson<TestPageResponse>();
        
        var result2 = await "/test"
            .GetGithubApiAsync(CancellationToken.None)
            .ReceiveJson<TestPageResponse>();

        result1.Should().BeSameAs(result2);
        result1.Should().Be(cachedResponse);
        result2.Should().Be(cachedResponse);
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
            .RespondWithJson(cachedResponse, (int)HttpStatusCode.OK, GetHeaders(new KeyValuePair<string, string>("ETag", "1")));
        
        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithHeader("If-None-Match", "1")
            .RespondWithJson(newResponse, (int)HttpStatusCode.OK, GetHeaders());

        var result1 = await "/test"
            .GetGithubApiAsync(CancellationToken.None)
            .ReceiveJson<TestPageResponse>();
        
        var result2 = await "/test"
            .GetGithubApiAsync(CancellationToken.None)
            .ReceiveJson<TestPageResponse>();

        result1.Should().NotBeSameAs(result2);
        result1.Should().Be(cachedResponse);
        result2.Should().Be(newResponse);
    }

    [Fact]
    public async Task GetGithubApiAsync_WhenCacheHitAndETagsDontMatch_ReturnMakeCall()
    {
        var response1 = new TestPageResponse(new List<TestPageItem>());
        var response2 = new TestPageResponse(new List<TestPageItem>());
        
        using var httpTest = new HttpTest();

        httpTest
            .RespondWithJson(response1, (int)HttpStatusCode.NotModified, GetHeaders(new KeyValuePair<string, string>("ETag", "1")))
            .RespondWithJson(response2, (int)HttpStatusCode.OK, GetHeaders(new KeyValuePair<string, string>("ETag", "1")));

        var result1 = await "/test"
            .GetGithubApiAsync(CancellationToken.None)
            .ReceiveJson<TestPageResponse>();
        
        var result2 = await "/test"
            .GetGithubApiAsync(CancellationToken.None)
            .ReceiveJson<TestPageResponse>();

        result1.Should().NotBeSameAs(result2);
        result1.Should().Be(response1);
        result2.Should().Be(response2);
    }
    
    [Fact]
    public async Task GetGithubApiAsync_WhenCacheHitAndNoETag_ReturnMakeCall()
    {
        var response1 = new TestPageResponse(new List<TestPageItem>());
        var response2 = new TestPageResponse(new List<TestPageItem>());
        
        using var httpTest = new HttpTest();

        httpTest
            .RespondWithJson(response1, (int)HttpStatusCode.OK, GetHeaders())
            .RespondWithJson(response2, (int)HttpStatusCode.OK, GetHeaders());

        var result1 = await "/test"
            .GetGithubApiAsync(CancellationToken.None)
            .ReceiveJson<TestPageResponse>();
        
        var result2 = await "/test"
            .GetGithubApiAsync(CancellationToken.None)
            .ReceiveJson<TestPageResponse>();

        result1.Should().NotBeSameAs(result2);
        result1.Should().Be(response1);
        result2.Should().Be(response2);
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