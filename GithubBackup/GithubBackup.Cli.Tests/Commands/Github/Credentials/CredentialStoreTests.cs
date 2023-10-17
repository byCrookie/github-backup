using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Core.Github.Credentials;
using NSubstitute;
using IEnvironment = GithubBackup.Core.Environment.IEnvironment;
using Environment = GithubBackup.Core.Environment.Environment;

namespace GithubBackup.Cli.Tests.Commands.Github.Credentials;

public class CredentialStoreTests
{
    private readonly CredentialStore _sut;
    private readonly MockFileSystem _fileSystem;
    private readonly IGithubTokenStore _githubTokenStore;
    private readonly IEnvironment _environment;

    public CredentialStoreTests()
    {
        _fileSystem = new MockFileSystem();
        _githubTokenStore = Substitute.For<IGithubTokenStore>();
        _environment = Substitute.For<IEnvironment>();
        
        _sut = new CredentialStore(_fileSystem, _githubTokenStore, _environment);
    }

    [Fact]
    public async Task LoadTokenAsync_AppDataDoesNotExist_TokenIsNull()
    {
        var path = _environment.Root("AppData");
        
        _environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
            .Returns(path);
        
        var result = await _sut.LoadTokenAsync(CancellationToken.None);
        
        result.Should().BeNull();
    }

    
}

	