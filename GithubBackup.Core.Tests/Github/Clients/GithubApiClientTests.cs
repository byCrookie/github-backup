using System.Net;
using AutoBogus;
using AwesomeAssertions;
using Flurl.Http;
using Flurl.Http.Testing;
using GithubBackup.Core.Github.Clients;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Tests.Utils;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Net.Http.Headers;
using NSubstitute;

namespace GithubBackup.Core.Tests.Github.Clients;

public class GithubApiClientTests
{
    private readonly GithubApiClient _sut;

    private const string Token = "token";
    private const string Url = "https://api.github.com/test";

    public GithubApiClientTests()
    {
        var tokenStore = Substitute.For<IGithubTokenStore>();

        tokenStore.GetAsync().Returns(Token);

        _sut = new GithubApiClient(
            new NullCache(),
            tokenStore,
            new DateTimeOffsetProvider(),
            new NullLogger<GithubApiClient>()
        );
    }

    [Fact]
    public async Task GetAsync_CallsUrl_ThenReturnResponse()
    {
        const string message = "Test";

        using var httpTest = new HttpTest();

        httpTest
            .ForCallsTo(Url)
            .WithVerb(HttpMethod.Get)
            .WithHeader(HeaderNames.Accept, "application/vnd.github.v3+json")
            .WithHeader(HeaderNames.UserAgent, "github-backup")
            .RespondWith(message, (int)HttpStatusCode.OK, GetHeaders());

        var result = await _sut.GetAsync("/test");

        result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        (await result.GetStringAsync()).Should().Be(message);
    }

    [Fact]
    public async Task ReceiveJsonPagedAsync_WhenHasPages_ThenReturnAllItems()
    {
        const string url = "https://api.github.com/test";
        const string pageParam = "page";
        const int pageSize = 2;

        var itemsBatch1 = new AutoFaker<TestPageItem>().Generate(pageSize);
        var itemsBatch2 = new AutoFaker<TestPageItem>().Generate(pageSize);
        var itemsBatch3 = new AutoFaker<TestPageItem>().Generate(pageSize - 1);

        var expectedItems = new List<TestPageItem>();
        expectedItems.AddRange(itemsBatch1);
        expectedItems.AddRange(itemsBatch2);
        expectedItems.AddRange(itemsBatch3);

        using var httpTest = new HttpTest();

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithQueryParam(pageParam, 1)
            .RespondWithJson(
                new TestPageResponse(itemsBatch1),
                (int)HttpStatusCode.OK,
                GetHeaders()
            );

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithQueryParam(pageParam, 2)
            .RespondWithJson(
                new TestPageResponse(itemsBatch2),
                (int)HttpStatusCode.OK,
                GetHeaders()
            );

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithQueryParam(pageParam, 3)
            .RespondWithJson(
                new TestPageResponse(itemsBatch3),
                (int)HttpStatusCode.OK,
                GetHeaders()
            );

        var result = await _sut.ReceiveJsonPagedAsync<TestPageResponse, TestPageItem>(
            "/test",
            pageSize,
            r => r.Items,
            null,
            CancellationToken.None
        );

        result.Should().BeEquivalentTo(expectedItems, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetJsonGithubApiPagedAsync_WhenHasNoPages_ThenReturnEmptyList()
    {
        const string url = "https://api.github.com/test";
        const int pageSize = 2;

        var itemsBatch = new List<TestPageItem>();

        using var httpTest = new HttpTest();

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(
                new TestPageResponse(itemsBatch),
                (int)HttpStatusCode.OK,
                GetHeaders()
            );

        var result = await _sut.ReceiveJsonPagedAsync<TestPageResponse, TestPageItem>(
            "/test",
            pageSize,
            r => r.Items,
            null,
            CancellationToken.None
        );

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGithubApiAsync_Reponse_Result()
    {
        const string url = "https://api.github.com/test";

        var items = new List<TestPageItem> { new(1, "name") };

        var expectedItems = new List<TestPageItem> { new(1, "name") };

        using var httpTest = new HttpTest();

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(new TestPageResponse(items), (int)HttpStatusCode.OK, GetHeaders());

        var result = await _sut.GetAsync("/test").ReceiveJson<TestPageResponse>();

        result
            .Items.Should()
            .BeEquivalentTo(expectedItems, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task PostJsonGithubApiAsync_Reponse_Result()
    {
        const string url = "https://api.github.com/test";

        var items = new List<TestPageItem> { new(1, "name") };

        var expectedItems = new List<TestPageItem> { new(1, "name") };

        using var httpTest = new HttpTest();

        var body = new { Test = "test" };

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Post)
            .WithRequestJson(body)
            .RespondWithJson(new TestPageResponse(items), (int)HttpStatusCode.OK, GetHeaders());

        var result = await _sut.PostJsonAsync("/test", body).ReceiveJson<TestPageResponse>();

        result
            .Items.Should()
            .BeEquivalentTo(expectedItems, options => options.WithStrictOrdering());
    }

    private static Dictionary<string, string> GetHeaders(
        params KeyValuePair<string, string>[] headers
    )
    {
        var allHeaders = new Dictionary<string, string>
        {
            { "x-ratelimit-remaining", "4999" },
            { "x-ratelimit-reset", "1614556800" },
        };

        foreach (var header in headers)
        {
            allHeaders.Add(header.Key, header.Value);
        }

        return allHeaders;
    }
}
