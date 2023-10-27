using System.Net;
using FluentAssertions;
using Flurl.Http.Testing;
using GithubBackup.Core.Github.Clients;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Utils;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GithubBackup.Core.Tests.Github.Clients;

public class GithubApiClientCacheTests
{
    private readonly GithubApiClient _sut;

    private readonly ILogger<GithubApiClient> _logger;

    private const string Token = "token";
    private const string TestId = "TEST-ID";

    public GithubApiClientCacheTests()
    {
        _logger = Substitute.For<ILogger<GithubApiClient>>();
        var tokenStore = Substitute.For<IGithubTokenStore>();

        tokenStore.Get().Returns(Token);

        _sut = new GithubApiClient(
            new MemoryCache(new MemoryCacheOptions()),
            tokenStore,
            new DateTimeOffsetProvider(),
            _logger
        );
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

        var result1 = await _sut.GetAsync("/test");
        var result2 = await _sut.GetAsync("/test");

        result1.Headers.GetRequired(TestId).Should().Be(nameof(cachedResponse));
        result2.Headers.GetRequired(TestId).Should().Be(nameof(cachedResponse));

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "Requesting https://api.github.com/test"),
            new LogEntry(LogLevel.Trace, "Sending GET request to https://api.github.com/test with content "),
            new LogEntry(LogLevel.Trace, """Received 200 response from https://api.github.com/test with content {"Items":\[]}"""),
            new LogEntry(LogLevel.Debug, "Cache - Caching response for GET - https://api.github.com/test"),
            new LogEntry(LogLevel.Debug, "Requesting https://api.github.com/test"),
            new LogEntry(LogLevel.Debug, "Cache - Returning cached response for GET - https://api.github.com/test")
        );
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

        var result1 = await _sut.GetAsync("/test");
        var result2 = await _sut.GetAsync("/test");

        result1.Headers.GetRequired(TestId).Should().Be(nameof(cachedResponse));
        result2.Headers.GetRequired(TestId).Should().Be(nameof(newResponse));

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "Requesting https://api.github.com/test"),
            new LogEntry(LogLevel.Trace, "Sending GET request to https://api.github.com/test with content "),
            new LogEntry(LogLevel.Trace, """Received 200 response from https://api.github.com/test with content {"Items":\[]}"""),
            new LogEntry(LogLevel.Debug, "Cache - Caching response for GET - https://api.github.com/test"),
            new LogEntry(LogLevel.Debug, "Requesting https://api.github.com/test"),
            new LogEntry(LogLevel.Debug, "Cache - Resource has changed, returning new response for GET - https://api.github.com/test"),
            new LogEntry(LogLevel.Trace, "Sending GET request to https://api.github.com/test with content "),
            new LogEntry(LogLevel.Trace, """Received 200 response from https://api.github.com/test with content {"Items":\[]}""")
        );
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

        var result1 = await _sut.GetAsync("/test");
        var result2 = await _sut.GetAsync("/test");

        result1.Headers.GetRequired(TestId).Should().Be(nameof(response1));
        result2.Headers.GetRequired(TestId).Should().Be(nameof(response2));

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "Requesting https://api.github.com/test"),
            new LogEntry(LogLevel.Trace, "Sending GET request to https://api.github.com/test with content "),
            new LogEntry(LogLevel.Trace, """Received 200 response from https://api.github.com/test with content {"Items":\[]}"""),
            new LogEntry(LogLevel.Debug, "Cache - Caching response for GET - https://api.github.com/test"),
            new LogEntry(LogLevel.Debug, "Requesting https://api.github.com/test"),
            new LogEntry(LogLevel.Debug, "Cache - Resource has changed, returning new response for GET - https://api.github.com/test"),
            new LogEntry(LogLevel.Trace, "Sending GET request to https://api.github.com/test with content "),
            new LogEntry(LogLevel.Trace, """Received 200 response from https://api.github.com/test with content {"Items":\[]}"""),
            new LogEntry(LogLevel.Debug, "Cache - Caching response for GET - https://api.github.com/test")
        );
    }

    [Fact]
    public async Task GetGithubApiAsync_WhenCacheHitAndNoETag_ReturnMakeCall()
    {
        var response1 = new TestPageResponse(new List<TestPageItem>());
        var response2 = new TestPageResponse(new List<TestPageItem>());

        using var httpTest = new HttpTest();

        httpTest
            .RespondWithJson(response1, (int)HttpStatusCode.OK,
                GetHeaders(new KeyValuePair<string, string>(TestId, nameof(response1))))
            .RespondWithJson(response2, (int)HttpStatusCode.OK,
                GetHeaders(new KeyValuePair<string, string>(TestId, nameof(response2))));

        var result1 = await _sut.GetAsync("/test");
        var result2 = await _sut.GetAsync("/test");

        result1.Headers.GetRequired(TestId).Should().Be(nameof(response1));
        result2.Headers.GetRequired(TestId).Should().Be(nameof(response2));

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "Requesting https://api.github.com/test"),
            new LogEntry(LogLevel.Trace, "Sending GET request to https://api.github.com/test with content "),
            new LogEntry(LogLevel.Trace, """Received 200 response from https://api.github.com/test with content {"Items":\[]}"""),
            new LogEntry(LogLevel.Debug, "Requesting https://api.github.com/test"),
            new LogEntry(LogLevel.Trace, "Sending GET request to https://api.github.com/test with content "),
            new LogEntry(LogLevel.Trace, """Received 200 response from https://api.github.com/test with content {"Items":\[]}""")
        );
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