using FluentAssertions;
using GithubBackup.Core.Github.Credentials;

namespace GithubBackup.Core.Tests.Credentials;

public class GithubTokenStoreTests
{
    [Fact]
    public void Get_TokenNotSet_ThrowException()
    {
        var result = GithubTokenStore.Get;
        result.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void Get_TokenSet_ReturnToken()
    {
        const string token = "Token";
        GithubTokenStore.Set(token);
        var result = GithubTokenStore.Get();
        result.Should().Be(token);
    }
}