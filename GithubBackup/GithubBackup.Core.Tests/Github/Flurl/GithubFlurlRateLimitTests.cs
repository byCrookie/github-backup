using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Flurl.Http;
using Flurl.Http.Testing;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Flurl;

namespace GithubBackup.Core.Tests.Github.Flurl;

public class GithubFlurlRateLimitTests : IDisposable
{
    private const string Token = "token";

    public GithubFlurlRateLimitTests()
    {
        GithubTokenStore.Set(Token);
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
            // .RespondWith(string.Empty, (int)HttpStatusCode.TooManyRequests, GetHeaders(new KeyValuePair<string, string>("retry-after", "1")))
            // .RespondWithJson(response, (int)HttpStatusCode.OK, GetHeaders(new KeyValuePair<string, string>("ETag", "1")));

        // var action = () => "/test"
        //     .GetGithubApiAsync(CancellationToken.None)
        //     .ReceiveJson<TestPageResponse>();
        //
        // (await action.ExecutionTimeAsync()).Should().BeCloseTo(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(0.1));
                
        // GithubFlurl.ClearCache();
        
        var result = await "/test"
            .GetGithubApiAsync(CancellationToken.None)
            .ReceiveJson<TestPageResponse>();

        result.Should().BeEquivalentTo(response);
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
            // .RespondWith(string.Empty, (int)HttpStatusCode.OK, GetHeaders(0, DateTimeOffset.UtcNow.AddSeconds(1)))
            // .RespondWithJson(response, (int)HttpStatusCode.OK, GetHeaders(100, DateTimeOffset.UtcNow.AddSeconds(1)));

        // var action = () => "/test"
        //     .GetGithubApiAsync(CancellationToken.None)
        //     .ReceiveJson<TestPageResponse>();
        //
        // (await action.ExecutionTimeAsync()).Should().BeCloseTo(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(0.1));
        
        // GithubFlurl.ClearCache();
        
        var result = await "/test"
            .GetGithubApiAsync(CancellationToken.None)
            .ReceiveJson<TestPageResponse>();
        
        result.Should().BeEquivalentTo(response);
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
        // .RespondWith(string.Empty, (int)HttpStatusCode.OK, GetHeaders(0, DateTimeOffset.UtcNow.AddSeconds(1)))
        // .RespondWithJson(response, (int)HttpStatusCode.OK, GetHeaders(100, DateTimeOffset.UtcNow.AddSeconds(1)));

        // var action = () => "/test"
        //     .GetGithubApiAsync(CancellationToken.None)
        //     .ReceiveJson<TestPageResponse>();
        //
        // (await action.ExecutionTimeAsync()).Should().BeCloseTo(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(0.1));
        
        // GithubFlurl.ClearCache();
        
        var result = await "/test"
            .GetGithubApiAsync(CancellationToken.None)
            .ReceiveJson<TestPageResponse>();
        
        result.Should().BeEquivalentTo(response);
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

    public void Dispose()
    {
        GithubFlurl.ClearCache();
    }
}