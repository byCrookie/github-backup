using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Manual;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Repositories;
using GithubBackup.Core.Github.Users;
using GithubBackup.Core.Utils;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console.Testing;

namespace GithubBackup.Cli.Tests.Commands.Github.Manual;

[UsesVerify]
public class ManualRunnerTests
{
    private readonly TestConsole _ansiConsole = new();
    private readonly ILogger<LoginRunner> _logger = Substitute.For<ILogger<LoginRunner>>();
    private readonly IMigrationService _migrationService = Substitute.For<IMigrationService>();
    private readonly IAuthenticationService _authenticationService = Substitute.For<IAuthenticationService>();
    private readonly IUserService _userService = Substitute.For<IUserService>();
    private readonly IRepositoryService _repositoryService = Substitute.For<IRepositoryService>();
    private readonly ICredentialStore _credentialStore = Substitute.For<ICredentialStore>();
    private readonly IFileSystem _fileSystem = new MockFileSystem();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    
    [Fact]
    public async Task RunAsync_HasValidTokenAndWantsToContinue_StopWhenNoRepositories()
    {
        var runner = CreateRunner();

        var user = new User("test", "test");

        _credentialStore.LoadTokenAsync(CancellationToken.None).Returns("token");
        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);
        _migrationService.GetMigrationsAsync(CancellationToken.None).Returns(new List<Migration>());
        
        _ansiConsole.Input.PushCharacter('y');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_HasValidTokenAndDoesNotWantToContinue_LoginAndStopWhenNoRepositories()
    {
        var runner = CreateRunner();

        var user = new User("test", "test");

        _credentialStore.LoadTokenAsync(CancellationToken.None).Returns("token");
        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);
        _migrationService.GetMigrationsAsync(CancellationToken.None).Returns(new List<Migration>());
        var deviceUserCodes = new DeviceAndUserCodes("device", "user", "uri", 0, 0);
        _authenticationService.RequestDeviceAndUserCodesAsync(CancellationToken.None).Returns(deviceUserCodes);
        var accessToken = new AccessToken("token", "bearer", "scope");
        _authenticationService.PollForAccessTokenAsync(deviceUserCodes.DeviceCode, deviceUserCodes.Interval, CancellationToken.None)
            .Returns(accessToken);
        
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();
        await _credentialStore.Received(1).StoreTokenAsync(accessToken.Token, CancellationToken.None);
        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_HasNoToken_LoginAndStopWhenNoRepositories()
    {
        var runner = CreateRunner();

        var user = new User("test", "test");

        _credentialStore.LoadTokenAsync(CancellationToken.None).Returns((string?)null);
        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);
        _migrationService.GetMigrationsAsync(CancellationToken.None).Returns(new List<Migration>());
        var deviceUserCodes = new DeviceAndUserCodes("device", "user", "uri", 0, 0);
        _authenticationService.RequestDeviceAndUserCodesAsync(CancellationToken.None).Returns(deviceUserCodes);
        var accessToken = new AccessToken("token", "bearer", "scope");
        _authenticationService.PollForAccessTokenAsync(deviceUserCodes.DeviceCode, deviceUserCodes.Interval, CancellationToken.None)
            .Returns(accessToken);
        
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();
        await _credentialStore.Received(1).StoreTokenAsync(accessToken.Token, CancellationToken.None);
        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_HasInvalidToken_LoginAndStopWhenNoRepositories()
    {
        var runner = CreateRunner();

        var user = new User("test", "test");

        _credentialStore.LoadTokenAsync(CancellationToken.None).Returns("token");
        _userService.WhoAmIAsync(CancellationToken.None).Returns(_ => throw new Exception(), _ => user);
        _migrationService.GetMigrationsAsync(CancellationToken.None).Returns(new List<Migration>());
        var deviceUserCodes = new DeviceAndUserCodes("device", "user", "uri", 0, 0);
        _authenticationService.RequestDeviceAndUserCodesAsync(CancellationToken.None).Returns(deviceUserCodes);
        var accessToken = new AccessToken("token", "bearer", "scope");
        _authenticationService.PollForAccessTokenAsync(deviceUserCodes.DeviceCode, deviceUserCodes.Interval, CancellationToken.None)
            .Returns(accessToken);
        
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();
        await _credentialStore.Received(1).StoreTokenAsync(accessToken.Token, CancellationToken.None);
        await Verify(_ansiConsole.Output);
    }
    
    private ManualBackupRunner CreateRunner()
    {
        var manualBackupArgs = new ManualBackupArgs();

        return new ManualBackupRunner(
            manualBackupArgs,
            _authenticationService,
            _migrationService,
            _userService,
            _repositoryService,
            _credentialStore,
            _fileSystem,
            _ansiConsole,
            _dateTimeProvider
        );
    }
}