using FluentAssertions;
using GithubBackup.Core.Github.Credentials;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GithubBackup.Core.Tests.Github.Credentials;

public class GithubTokenStoreTests
{
    private readonly GithubTokenStore _sut;

    public GithubTokenStoreTests()
    {
        var logger = Substitute.For<ILogger<GithubTokenStore>>();

        _sut = new GithubTokenStore(logger);
    }

    [Fact]
    public async Task GetAsync_TokenNotSet_ThrowException()
    {
        var result = _sut.GetAsync;
       await result.Should().ThrowAsync<InvalidOperationException>();
    }
    
    [Fact]
    public async Task GetAsync_TokenSet_ReturnToken()
    {
        const string token = "Token";
        await _sut.SetAsync(token);
        var result = await _sut.GetAsync();
        result.Should().Be(token);
    }
}