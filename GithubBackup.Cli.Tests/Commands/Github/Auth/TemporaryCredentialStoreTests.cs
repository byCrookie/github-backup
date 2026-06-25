using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using AwesomeAssertions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Environment = GithubBackup.Core.Environment.Environment;
using IEnvironment = GithubBackup.Core.Environment.IEnvironment;

namespace GithubBackup.Cli.Tests.Commands.Github.Auth;

public class TemporaryCredentialStoreTests
{
    private readonly TemporaryCredentialStore _sut;
    private readonly MockFileSystem _fileSystem;
    private readonly IEnvironment _environment;
    private readonly ILogger<TemporaryCredentialStore> _logger;

    private const string AppDirectory = "github-backup";
    private const string AppDataDirectory = "AppData";
    private const string TokenFileName = "temporary-token.json";

    public TemporaryCredentialStoreTests()
    {
        _fileSystem = new MockFileSystem();
        _environment = Substitute.For<IEnvironment>();
        _logger = Substitute.For<ILogger<TemporaryCredentialStore>>();

        _sut = new TemporaryCredentialStore(_fileSystem, _environment, _logger);
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
        var path = CreateAppDataPath();

        var result = await _sut.LoadTokenAsync(CancellationToken.None);

        result.Should().BeNull();

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "AppData path is (.*)"),
            new LogEntry(LogLevel.Debug, "Temporary token path is (.*)"),
            new LogEntry(LogLevel.Debug, "Path (.*) exists"),
            new LogEntry(LogLevel.Debug, "Temporary token file (.*) does not exist")
        );
    }

    [Fact]
    public async Task LoadTokenAsync_TokenFileDoesExistButEmpty_TokenIsNull()
    {
        var path = CreateAppDataPath();
        var tokenFile = GetTokenFile(path);
        await _fileSystem.File.WriteAllTextAsync(tokenFile, string.Empty, TestContext.Current.CancellationToken);

        var result = await _sut.LoadTokenAsync(CancellationToken.None);

        result.Should().BeNull();

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "AppData path is (.*)"),
            new LogEntry(LogLevel.Debug, "Temporary token path is (.*)"),
            new LogEntry(LogLevel.Debug, "Path (.*) exists"),
            new LogEntry(LogLevel.Debug, "Loading temporary token from (.*)"),
            new LogEntry(LogLevel.Warning, "Temporary token file (.*) is empty")
        );
    }

    [Fact]
    public async Task LoadTokenAsync_TokenFileDoesExistAndToken_ReturnToken()
    {
        var path = CreateAppDataPath();
        var tokenFile = GetTokenFile(path);
        var credential = new TemporaryCredential(
            "test",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        );
        await _fileSystem.File.WriteAllTextAsync(tokenFile, JsonSerializer.Serialize(credential), TestContext.Current.CancellationToken);

        var result = await _sut.LoadTokenAsync(CancellationToken.None);

        result.Should().Be(credential);
    }

    [Fact]
    public async Task StoreTokenAsync_TokenFileDoesNotExist_CreatePlaintextFileAndWrite()
    {
        var path = CreateAppDataPath();
        var expiresAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        await _sut.StoreTokenAsync("test", expiresAt, CancellationToken.None);

        var tokenFile = GetTokenFile(path);
        _fileSystem.File.Exists(tokenFile).Should().BeTrue();
        (await _fileSystem.File.ReadAllTextAsync(tokenFile, TestContext.Current.CancellationToken)).Should().Contain("test");
        (await _sut.LoadTokenAsync(CancellationToken.None)).Should().Be(
            new TemporaryCredential("test", expiresAt)
        );
    }

    [Fact]
    public async Task DeleteTokenAsync_TokenFileExists_DeleteFile()
    {
        var path = CreateAppDataPath();
        var tokenFile = GetTokenFile(path);
        await _fileSystem.File.WriteAllTextAsync(tokenFile, "test", TestContext.Current.CancellationToken);

        await _sut.DeleteTokenAsync(CancellationToken.None);

        _fileSystem.File.Exists(tokenFile).Should().BeFalse();
    }

    private string CreateAppDataPath()
    {
        var path = new Environment(_fileSystem).Root(AppDataDirectory);
        _environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData).Returns(path);

        var githubBackupPath = _fileSystem.Path.Combine(path.FullName, AppDirectory);
        _fileSystem.Directory.CreateDirectory(githubBackupPath);
        return path.FullName;
    }

    private string GetTokenFile(string appDataPath)
    {
        var githubBackupPath = _fileSystem.Path.Combine(appDataPath, AppDirectory);
        return _fileSystem.Path.Combine(githubBackupPath, TokenFileName);
    }
}
