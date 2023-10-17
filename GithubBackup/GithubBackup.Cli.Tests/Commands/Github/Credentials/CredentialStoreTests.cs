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
    
    private const string EncryptedToken = "yyCJ7xAPG8Zd/aDydAqK0w==";
    private const string DecryptedToken = "test";
    private const string AppDirectory = "github-backup";
    private const string AppDataDirectory = "AppData";
    private const string TokenFileName = "token";

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
        var path = new Environment(_fileSystem).Root(AppDataDirectory);
        
        _environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
            .Returns(path);
        
        var result = await _sut.LoadTokenAsync(CancellationToken.None);
        
        result.Should().BeNull();
        _githubTokenStore.Received(1).Set(null);
    }

    [Fact]
    public async Task LoadTokenAsync_TokenFileDoesNotExist_TokenIsNull()
    {
        var path = new Environment(_fileSystem).Root(AppDataDirectory);
        
        _environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
            .Returns(path);

        var githubBackupPath = _fileSystem.Path.Combine(path.FullName, AppDirectory);
        _fileSystem.Directory.CreateDirectory(githubBackupPath);
        
        var result = await _sut.LoadTokenAsync(CancellationToken.None);
        
        result.Should().BeNull();
        _githubTokenStore.Received(1).Set(null);
    }
    
    [Fact]
    public async Task LoadTokenAsync_TokenFileDoesExistButEmpty_TokenIsNull()
    {
        var path = new Environment(_fileSystem).Root(AppDataDirectory);
        
        _environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
            .Returns(path);

        var githubBackupPath = _fileSystem.Path.Combine(path.FullName, AppDirectory);
        _fileSystem.Directory.CreateDirectory(githubBackupPath);
        var tokenFile = _fileSystem.Path.Combine(githubBackupPath, TokenFileName);
        await _fileSystem.File.WriteAllTextAsync(tokenFile, string.Empty);
        
        var result = await _sut.LoadTokenAsync(CancellationToken.None);
        
        result.Should().BeNull();
        _githubTokenStore.Received(1).Set(null);
    }
    
    [Fact]
    public async Task LoadTokenAsync_TokenFileDoesExistAndToken_Token()
    {
        var path = new Environment(_fileSystem).Root(AppDataDirectory);
        
        _environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
            .Returns(path);

        var githubBackupPath = _fileSystem.Path.Combine(path.FullName, AppDirectory);
        _fileSystem.Directory.CreateDirectory(githubBackupPath);
        var tokenFile = _fileSystem.Path.Combine(githubBackupPath, TokenFileName);
        await _fileSystem.File.WriteAllTextAsync(tokenFile, EncryptedToken);
        
        var result = await _sut.LoadTokenAsync(CancellationToken.None);
        
        result.Should().Be(DecryptedToken);
        _githubTokenStore.Received(1).Set(DecryptedToken);
    }
    
    [Fact]
    public async Task StoreTokenAsync_TokenFileDoesNotExist_CreateFileAndWrite()
    {
        var path = new Environment(_fileSystem).Root(AppDataDirectory);
        
        _environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
            .Returns(path);

        var githubBackupPath = _fileSystem.Path.Combine(path.FullName, AppDirectory);
        _fileSystem.Directory.CreateDirectory(githubBackupPath);
        
        await _sut.StoreTokenAsync(DecryptedToken, CancellationToken.None);
        
        var tokenFile = _fileSystem.Path.Combine(githubBackupPath, TokenFileName);
        _fileSystem.File.Exists(tokenFile).Should().BeTrue();
        (await _fileSystem.File.ReadAllTextAsync(tokenFile)).Should().Be(EncryptedToken);
        _githubTokenStore.Received(1).Set(DecryptedToken);
    }
    
    [Fact]
    public async Task StoreTokenAsync_AppDirectoryDoesNotExist_DoNotStoreToken()
    {
        var path = new Environment(_fileSystem).Root(AppDataDirectory);
        
        _environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
            .Returns(path);
        
        await _sut.StoreTokenAsync(DecryptedToken, CancellationToken.None);
        
        var githubBackupPath = _fileSystem.Path.Combine(path.FullName, AppDirectory);
        var tokenFile = _fileSystem.Path.Combine(githubBackupPath, TokenFileName);
        _fileSystem.File.Exists(tokenFile).Should().BeFalse();
        _githubTokenStore.Received(1).Set(null);
    }
}

	