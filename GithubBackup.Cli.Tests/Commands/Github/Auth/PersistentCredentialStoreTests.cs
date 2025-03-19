using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Environment = GithubBackup.Core.Environment.Environment;
using IEnvironment = GithubBackup.Core.Environment.IEnvironment;

namespace GithubBackup.Cli.Tests.Commands.Github.Auth;

public class PersistentCredentialStoreTests
{
    private readonly PersistentCredentialStore _sut;
    private readonly MockFileSystem _fileSystem;
    private readonly IEnvironment _environment;
    private readonly ILogger<PersistentCredentialStore> _logger;

    private const string EncryptedToken = "yyCJ7xAPG8Zd/aDydAqK0w==";
    private const string DecryptedToken = "test";
    private const string AppDirectory = "github-backup";
    private const string AppDataDirectory = "AppData";
    private const string TokenFileName = "token";

    public PersistentCredentialStoreTests()
    {
        _fileSystem = new MockFileSystem();
        _environment = Substitute.For<IEnvironment>();
        _logger = Substitute.For<ILogger<PersistentCredentialStore>>();

        _sut = new PersistentCredentialStore(_fileSystem, _environment, _logger);
    }

    [Fact]
    public async Task LoadTokenAsync_AppDataDoesNotExist_TokenIsNull()
    {
        var path = new Environment(_fileSystem).Root(AppDataDirectory);

        _environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData).Returns(path);

        var result = await _sut.LoadTokenAsync(CancellationToken.None);

        result.Should().BeNull();

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "AppData path is (.*)"),
            new LogEntry(LogLevel.Warning, "AppData path (.*) does not exist")
        );
    }

    [Fact]
    public async Task LoadTokenAsync_TokenFileDoesNotExist_TokenIsNull()
    {
        var path = new Environment(_fileSystem).Root(AppDataDirectory);

        _environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData).Returns(path);

        var githubBackupPath = _fileSystem.Path.Combine(path.FullName, AppDirectory);
        _fileSystem.Directory.CreateDirectory(githubBackupPath);

        var result = await _sut.LoadTokenAsync(CancellationToken.None);

        result.Should().BeNull();

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "AppData path is (.*)"),
            new LogEntry(LogLevel.Debug, "Token path is (.*)"),
            new LogEntry(LogLevel.Debug, "Path (.*) exists"),
            new LogEntry(LogLevel.Debug, "Token file (.*) does not exist")
        );
    }

    [Fact]
    public async Task LoadTokenAsync_TokenFileDoesExistButEmpty_TokenIsNull()
    {
        var path = new Environment(_fileSystem).Root(AppDataDirectory);

        _environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData).Returns(path);

        var githubBackupPath = _fileSystem.Path.Combine(path.FullName, AppDirectory);
        _fileSystem.Directory.CreateDirectory(githubBackupPath);
        var tokenFile = _fileSystem.Path.Combine(githubBackupPath, TokenFileName);
        await _fileSystem.File.WriteAllTextAsync(tokenFile, string.Empty);

        var result = await _sut.LoadTokenAsync(CancellationToken.None);

        result.Should().BeNull();

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "AppData path is (.*)"),
            new LogEntry(LogLevel.Debug, "Token path is (.*)"),
            new LogEntry(LogLevel.Debug, "Path (.*) exists"),
            new LogEntry(LogLevel.Debug, "Loading encrypted token from (.*)"),
            new LogEntry(LogLevel.Warning, "Token file (.*) is empty")
        );
    }

    [Fact]
    public async Task LoadTokenAsync_TokenFileDoesExistAndToken_Token()
    {
        var path = new Environment(_fileSystem).Root(AppDataDirectory);

        _environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData).Returns(path);

        var githubBackupPath = _fileSystem.Path.Combine(path.FullName, AppDirectory);
        _fileSystem.Directory.CreateDirectory(githubBackupPath);
        var tokenFile = _fileSystem.Path.Combine(githubBackupPath, TokenFileName);
        await _fileSystem.File.WriteAllTextAsync(tokenFile, EncryptedToken);

        var result = await _sut.LoadTokenAsync(CancellationToken.None);

        result.Should().Be(DecryptedToken);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "AppData path is (.*)"),
            new LogEntry(LogLevel.Debug, "Token path is (.*)"),
            new LogEntry(LogLevel.Debug, "Path (.*) exists"),
            new LogEntry(LogLevel.Debug, "Loading encrypted token from (.*)")
        );
    }

    [Fact]
    public async Task StoreTokenAsync_TokenFileDoesNotExist_CreateFileAndWrite()
    {
        var path = new Environment(_fileSystem).Root(AppDataDirectory);

        _environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData).Returns(path);

        var githubBackupPath = _fileSystem.Path.Combine(path.FullName, AppDirectory);
        _fileSystem.Directory.CreateDirectory(githubBackupPath);

        await _sut.StoreTokenAsync(DecryptedToken, CancellationToken.None);

        var tokenFile = _fileSystem.Path.Combine(githubBackupPath, TokenFileName);
        _fileSystem.File.Exists(tokenFile).Should().BeTrue();
        (await _fileSystem.File.ReadAllTextAsync(tokenFile)).Should().Be(EncryptedToken);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "AppData path is (.*)"),
            new LogEntry(LogLevel.Debug, "Token path is (.*)"),
            new LogEntry(LogLevel.Debug, "Path (.*) exists"),
            new LogEntry(LogLevel.Debug, "Storing encrypted token in (.*)")
        );
    }

    [Fact]
    public async Task StoreTokenAsync_AppDirectoryDoesNotExist_DoNotStoreToken()
    {
        var path = new Environment(_fileSystem).Root(AppDataDirectory);

        _environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData).Returns(path);

        var action = () => _sut.StoreTokenAsync(DecryptedToken, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();

        var githubBackupPath = _fileSystem.Path.Combine(path.FullName, AppDirectory);
        var tokenFile = _fileSystem.Path.Combine(githubBackupPath, TokenFileName);
        _fileSystem.File.Exists(tokenFile).Should().BeFalse();

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "AppData path is (.*)"),
            new LogEntry(LogLevel.Warning, "AppData path (.*) does not exist")
        );
    }
}
