using FluentAssertions;
using Flurl.Http.Testing;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Flurl;
using GithubBackup.Core.Github.Users;
using Microsoft.Net.Http.Headers;

namespace GithubBackup.Core.Tests.Github.Users;

public class UserServiceTests
{
    private readonly UserService _sut;
    private const string Token = "token";

    public UserServiceTests()
    {
        GithubTokenStore.Set(Token);

        _sut = new UserService();
    }

    [Fact]
    public async Task WhoAmIAsync_WhenReturnUser_ThenReturnUser()
    {
        const string login = "login";
        const string name = "name";

        using var httpTest = new HttpTest();

        httpTest
            .ForCallsTo("/user".RequestApi().Url)
            .WithVerb(HttpMethod.Get)
            .WithHeader(HeaderNames.Authorization, $"Bearer {Token}")
            .RespondWithJson(new UserResponse(login, name), 200, new Dictionary<string, object>
            {
                { "x-ratelimit-remaining", "4999" },
                { "x-ratelimit-reset", "1614556800" }
            });

        var result = await _sut.WhoAmIAsync(CancellationToken.None);

        result.Login.Should().Be(login);
        result.Name.Should().Be(name);
    }
}