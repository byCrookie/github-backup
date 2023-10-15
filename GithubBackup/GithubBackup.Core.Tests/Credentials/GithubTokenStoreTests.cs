using FluentAssertions;
using GithubBackup.Core.Github.Credentials;

namespace GithubBackup.Core.Tests.Credentials;

public class GithubTokenStoreTests
{
    private readonly GithubTokenStore _sut = new();

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