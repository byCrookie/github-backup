using FluentAssertions;
using GithubBackup.Core.Github.Clients;
using GithubBackup.Core.Github.Users;
using GithubBackup.TestUtils.Flurl;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GithubBackup.Core.Tests.Github.Users;

public class UserServiceTests
{
    private readonly UserService _sut;

    private readonly IGithubApiClient _githubApiClient;

    public UserServiceTests()
    {
        _githubApiClient = Substitute.For<IGithubApiClient>();
        var logger = Substitute.For<ILogger<UserService>>();

        _sut = new UserService(_githubApiClient, logger);
    }

    [Fact]
    public async Task WhoAmIAsync_WhenReturnUser_ThenReturnUser()
    {
        const string login = "login";
        const string name = "name";

        var reponse = new UserResponse(login, name).ToFlurlJsonResponse();

        _githubApiClient.GetAsync("/user", null, Arg.Any<CancellationToken>()).Returns(reponse);

        var result = await _sut.WhoAmIAsync(CancellationToken.None);

        result.Login.Should().Be(login);
        result.Name.Should().Be(name);
    }
}
