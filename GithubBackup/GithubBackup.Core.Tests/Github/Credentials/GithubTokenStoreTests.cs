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
    public void Get_TokenNotSet_ThrowException()
    {
        var result = _sut.Get;
        result.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void Get_TokenSet_ReturnToken()
    {
        const string token = "Token";
        _sut.Set(token);
        var result = _sut.Get();
        result.Should().Be(token);
    }
}