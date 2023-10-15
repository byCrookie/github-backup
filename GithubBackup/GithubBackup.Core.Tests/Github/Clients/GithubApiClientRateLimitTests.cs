using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Flurl.Http;
using Flurl.Http.Testing;
using GithubBackup.Core.Github.Clients;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Tests.Utils;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GithubBackup.Core.Tests.Github.Clients;

public class GithubApiClientRateLimitTests
{
    private readonly GithubApiClient _sut;

    private readonly ILogger<GithubApiClient> _logger;

    private const string Token = "token";

    public GithubApiClientRateLimitTests()
    {
        var store = new GithubTokenStore();
        store.Set(Token);

        _logger = Substitute.For<ILogger<GithubApiClient>>();

        _sut = new GithubApiClient(new NullCache(), store, new DateTimeOffsetProvider(), _logger);
    }

    [Fact]
    public async Task GetGithubApiAsync_WhenHitSecondaryRateLimits_ReturnAfterRetry()
    {
        var response = new TestPageResponse(new List<TestPageItem>());

        using var httpTest = new HttpTest();

        httpTest
            .RespondWith(string.Empty, (int)HttpStatusCode.TooManyRequests, GetHeaders(new KeyValuePair<string, string>("retry-after", "1")))
            .RespondWithJson(response, (int)HttpStatusCode.OK, GetHeaders())
            .SimulateException(new UnreachableException());

        var result = await _sut
            .GetAsync("/test")
            .ReceiveJson<TestPageResponse>();

        result.Should().BeEquivalentTo(response);
        
        _logger.VerifyLogs(
            (LogLevel.Debug, "RetryAfter - Delaying for (.*) before retrying request to GET - https://api.github.com/test")
        );
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
        
        _logger.VerifyLogs(
            (LogLevel.Debug, "RateLimit - Delaying for (.*) before retrying request to GET - https://api.github.com/test")
        );
    }

    [Fact]
    public async Task GetGithubApiAsync_WhenHitException_ReturnAfter3Attempts()
    {
        var response = new TestPageResponse(new List<TestPageItem>());

        using var httpTest = new HttpTest();

        httpTest
            .RespondWithJson(response, (int)HttpStatusCode.RequestTimeout, GetHeaders())
            .RespondWithJson(response, (int)HttpStatusCode.InternalServerError, GetHeaders())
            .RespondWithJson(response, (int)HttpStatusCode.ServiceUnavailable, GetHeaders())
            .RespondWithJson(response, (int)HttpStatusCode.OK, GetHeaders())
            .SimulateException(new UnreachableException());

        var result = await _sut
            .GetAsync("/test")
            .ReceiveJson<TestPageResponse>();

        result.Should().BeEquivalentTo(response);
        
        _logger.VerifyLogs(
            (LogLevel.Debug, "Retry Attempt 0 - Delaying for (.*) before retrying request to GET - https://api.github.com/test"),
            (LogLevel.Debug, "Retry Attempt 1 - Delaying for (.*) before retrying request to GET - https://api.github.com/test"),
            (LogLevel.Debug, "Retry Attempt 2 - Delaying for (.*) before retrying request to GET - https://api.github.com/test")
        );
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