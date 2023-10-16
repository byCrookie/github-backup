using FluentAssertions;
using Flurl.Http;
using GithubBackup.Core.Github.Clients;
using GithubBackup.Core.Github.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GithubBackup.Core.Tests.Github.Repositories;

public class RepositoryServiceTests
{
    private readonly RepositoryService _sut;
    
    private readonly IGithubApiClient _githubApiClient;

    public RepositoryServiceTests()
    {        
        _githubApiClient = Substitute.For<IGithubApiClient>();
        var logger = Substitute.For<ILogger<RepositoryService>>();
        
        _sut = new RepositoryService(_githubApiClient, logger);
    }

    [Fact]
    public async Task GetRepositoriesAsync_Recevies_Return()
    {
        var ct = CancellationToken.None;
        
        _githubApiClient.ReceiveJsonPagedAsync(
            "/user/repos",
            100,
            Arg.Any<Func<List<RepositoryResponse>, List<RepositoryResponse>>>(),
            Arg.Any<Action<IFlurlRequest>?>(),
            ct
        ).Returns(new List<RepositoryResponse>
        {
            new("Test 1"),
            new("Test 2")
        });
        
        var result = await _sut.GetRepositoriesAsync(new RepositoryOptions(RepositoryType.All), ct);
        
        result.Should().BeEquivalentTo(new List<Repository>
        {
            new("Test 1"),
            new("Test 2")
        });
    }
}