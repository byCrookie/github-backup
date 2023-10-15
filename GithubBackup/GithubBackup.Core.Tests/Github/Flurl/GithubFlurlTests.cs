using System.Net;
using AutoBogus;
using FluentAssertions;
using Flurl;
using Flurl.Http;
using Flurl.Http.Testing;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Flurl;
using GithubBackup.Core.Utils;
using Microsoft.Net.Http.Headers;

namespace GithubBackup.Core.Tests.Github.Flurl;

public class GithubFlurlTests : IDisposable
{
    private const string Token = "token";

    public GithubFlurlTests()
    {
        GithubTokenStore.Set(Token);
        GithubFlurl.ClearCache();
    }

    [Fact]
    public void RequestApi_Create_ThenReturnApiRequest()
    {
        var result = "/test".RequestApi();

        result.Should().NotBeNull();
        result.Url.Should().Be(new Url("https://api.github.com/test"));
        result.Client.Headers.GetRequired(HeaderNames.Accept).Should().Be("application/vnd.github.v3+json");
        result.Client.Headers.GetRequired(HeaderNames.UserAgent).Should().Be("github-backup");
    }

    [Fact]
    public void RequestApi_Create_ThenReturnWebRequest()
    {
        var result = "/test".RequestWeb();

        result.Should().NotBeNull();
        result.Url.Should().Be(new Url("https://github.com/test"));
        result.Client.Headers.GetRequired(HeaderNames.Accept).Should().Be("application/vnd.github.v3+json");
        result.Client.Headers.GetRequired(HeaderNames.UserAgent).Should().Be("github-backup");
    }

    [Fact]
    public void RequestApi_CreateFormUrl_ThenReturnApiRequest()
    {
        var result = new Url("/test").RequestApi();

        result.Should().NotBeNull();
        result.Url.Should().Be(new Url("https://api.github.com/test"));
        result.Client.Headers.GetRequired(HeaderNames.Accept).Should().Be("application/vnd.github.v3+json");
        result.Client.Headers.GetRequired(HeaderNames.UserAgent).Should().Be("github-backup");
    }

    [Fact]
    public void RequestApi_CreateFromUrl_ThenReturnWebRequest()
    {
        var result = new Url("/test").RequestWeb();

        result.Should().NotBeNull();
        result.Url.Should().Be(new Url("https://github.com/test"));
        result.Client.Headers.GetRequired(HeaderNames.Accept).Should().Be("application/vnd.github.v3+json");
        result.Client.Headers.GetRequired(HeaderNames.UserAgent).Should().Be("github-backup");
    }

    [Fact]
    public async Task GetJsonGithubApiPagedAsync_WhenHasPages_ThenReturnAllItems()
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
            .RespondWithJson(new TestPageResponse(itemsBatch1), (int)HttpStatusCode.OK, GetHeaders(new KeyValuePair<string, string>("ETag", "1")));

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithQueryParam(pageParam, 2)
            .RespondWithJson(new TestPageResponse(itemsBatch2), (int)HttpStatusCode.OK, GetHeaders(new KeyValuePair<string, string>("ETag", "2")));

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .WithQueryParam(pageParam, 3)
            .RespondWithJson(new TestPageResponse(itemsBatch3), (int)HttpStatusCode.OK, GetHeaders(new KeyValuePair<string, string>("ETag", "3")));

        var result = await "/test"
            .RequestApi()
            .GetJsonGithubApiPagedAsync<TestPageResponse, TestPageItem>(
                pageSize,
                r => r.Items,
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
            .RespondWithJson(new TestPageResponse(itemsBatch), (int)HttpStatusCode.OK, GetHeaders());

        var result = await "/test"
            .RequestApi()
            .GetJsonGithubApiPagedAsync<TestPageResponse, TestPageItem>(
                pageSize,
                r => r.Items,
                CancellationToken.None
            );

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGithubApiAsync_Reponse_Result()
    {
        const string url = "https://api.github.com/test";

        var items = new List<TestPageItem>
        {
            new(1, "name")
        };

        var expectedItems = new List<TestPageItem>
        {
            new(1, "name")
        };

        using var httpTest = new HttpTest();

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Get)
            .RespondWithJson(new TestPageResponse(items), (int)HttpStatusCode.OK, GetHeaders());

        var result = await "/test"
            .GetGithubApiAsync(CancellationToken.None)
            .ReceiveJson<TestPageResponse>();

        result.Items.Should().BeEquivalentTo(expectedItems, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task PostJsonGithubApiAsync_Reponse_Result()
    {
        const string url = "https://api.github.com/test";

        var items = new List<TestPageItem>
        {
            new(1, "name")
        };

        var expectedItems = new List<TestPageItem>
        {
            new(1, "name")
        };

        using var httpTest = new HttpTest();

        var body = new { Test = "test" };

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Post)
            .WithRequestJson(body)
            .RespondWithJson(new TestPageResponse(items), (int)HttpStatusCode.OK, GetHeaders());

        var result = await "/test"
            .PostJsonGithubApiAsync(body, CancellationToken.None)
            .ReceiveJson<TestPageResponse>();

        result.Items.Should().BeEquivalentTo(expectedItems, options => options.WithStrictOrdering());
    }
    
    [Fact]
    public async Task PostJsonGithubWebAsync_Reponse_Result()
    {
        const string url = "https://github.com/test";

        var items = new List<TestPageItem>
        {
            new(1, "name")
        };

        var expectedItems = new List<TestPageItem>
        {
            new(1, "name")
        };

        using var httpTest = new HttpTest();

        var body = new { Test = "test" };

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Post)
            .WithRequestJson(body)
            .RespondWithJson(new TestPageResponse(items), (int)HttpStatusCode.OK, GetHeaders());

        var result = await "/test"
            .PostJsonGithubWebAsync(body, CancellationToken.None)
            .ReceiveJson<TestPageResponse>();

        result.Items.Should().BeEquivalentTo(expectedItems, options => options.WithStrictOrdering());
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
        GithubTokenStore.Set(null);
        GithubFlurl.ClearCache();
    }
}