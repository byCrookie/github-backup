using AwesomeAssertions;
using Flurl.Http;
using Flurl.Http.Testing;
using GithubBackup.Core.Github.Clients;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GithubBackup.Core.Tests.Github.Clients;

public class GithubWebClientTests
{
    private readonly GithubWebClient _sut;

    public GithubWebClientTests()
    {
        var logger = Substitute.For<ILogger<GithubWebClient>>();

        _sut = new GithubWebClient(logger);
    }

    [Fact]
    public async Task PostJsonGithubWebAsync_Reponse_Result()
    {
        const string url = "https://github.com/test";

        var items = new List<TestPageItem> { new(1, "name") };

        var expectedItems = new List<TestPageItem> { new(1, "name") };

        using var httpTest = new HttpTest();

        var body = new { Test = "test" };

        httpTest
            .ForCallsTo(url)
            .WithVerb(HttpMethod.Post)
            .WithRequestJson(body)
            .RespondWithJson(new TestPageResponse(items));

        var result = await _sut.PostJsonAsync("/test", body).ReceiveJson<TestPageResponse>();

        result
            .Items.Should()
            .BeEquivalentTo(expectedItems, options => options.WithStrictOrdering());
    }
}
