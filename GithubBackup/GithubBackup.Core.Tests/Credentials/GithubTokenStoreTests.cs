using FluentAssertions;
using GithubBackup.Core.Github.Credentials;

namespace GithubBackup.Core.Tests.Credentials;

public class GithubTokenStoreTests : IDisposable
{
    public GithubTokenStoreTests()
    {
        GithubTokenStore.Set(null);
    }
    
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

    public void Dispose()
    {
        GithubTokenStore.Set(null);
    }
}