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
using Meziantou.Xunit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console.Testing;
using Environment = GithubBackup.Core.Environment.Environment;

namespace GithubBackup.Cli.Tests.Commands.Github.Manual;

[UsesVerify]
[DisableParallelization]
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

    public ManualRunnerTests()
    {
        _ansiConsole.Profile.Capabilities.Interactive = true;
    }

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

    [Fact]
    public async Task RunAsync_NoMigrations_DoNotBackup()
    {
        var runner = CreateRunner();

        var user = new User("test", "test");

        _credentialStore.LoadTokenAsync(CancellationToken.None).Returns("token");

        _ansiConsole.Input.PushCharacter('y');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        var migrations = new List<Migration>();

        _dateTimeProvider.Now.Returns(new DateTime(2020, 1, 11));

        _migrationService.GetMigrationsAsync(CancellationToken.None)
            .Returns(migrations);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_NoValidMigrations_DoNotBackup()
    {
        var runner = CreateRunner();

        var user = new User("test", "test");

        _credentialStore.LoadTokenAsync(CancellationToken.None).Returns("token");

        _ansiConsole.Input.PushCharacter('y');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        var migrations = new List<Migration>
        {
            new(1, MigrationState.Exported, new DateTime(2020, 1, 1)),
            new(2, MigrationState.Exporting, new DateTime(2020, 1, 6)),
            new(3, MigrationState.Pending, new DateTime(2020, 1, 10)),
        };

        _dateTimeProvider.Now.Returns(new DateTime(2020, 1, 11));

        _migrationService.GetMigrationsAsync(CancellationToken.None)
            .Returns(migrations);

        _migrationService.GetMigrationAsync(1, CancellationToken.None)
            .Returns(migrations[0]);

        _migrationService.GetMigrationAsync(2, CancellationToken.None)
            .Returns(migrations[1]);

        _migrationService.GetMigrationAsync(3, CancellationToken.None)
            .Returns(migrations[2]);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_ValidMigrations_BackupSelected()
    {
        var runner = CreateRunner();

        var user = new User("test", "test");

        _credentialStore.LoadTokenAsync(CancellationToken.None).Returns("token");

        _ansiConsole.Input.PushCharacter('y');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        var migrations = new List<Migration>
        {
            new(1, MigrationState.Exported, new DateTime(2020, 1, 1)),
            new(2, MigrationState.Exported, new DateTime(2020, 1, 6)),
            new(3, MigrationState.Exported, new DateTime(2020, 1, 10)),
        };

        _dateTimeProvider.Now.Returns(new DateTime(2020, 1, 11));

        const int id = 3;

        _migrationService.GetMigrationsAsync(CancellationToken.None)
            .Returns(migrations);

        _migrationService.GetMigrationAsync(1, CancellationToken.None)
            .Returns(migrations[0]);

        _migrationService.GetMigrationAsync(2, CancellationToken.None)
            .Returns(migrations[1]);

        _migrationService.GetMigrationAsync(id, CancellationToken.None)
            .Returns(migrations[2]);

        _ansiConsole.Input.PushKey(ConsoleKey.DownArrow);
        _ansiConsole.Input.PushKey(ConsoleKey.Spacebar);
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        const string destinationPath = "test1";
        var pathInvalid = new Environment(_fileSystem).Root(destinationPath);
        _ansiConsole.Input.PushText(pathInvalid.FullName);
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        const string destinationValidPath = "test2";
        var pathValid = new Environment(_fileSystem).Root(destinationValidPath);
        _fileSystem.Directory.CreateDirectory(pathValid.FullName);
        _ansiConsole.Input.PushText(pathValid.FullName);
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        _migrationService.DownloadMigrationAsync(Arg.Is<DownloadMigrationOptions>(o => o.Id == id), CancellationToken.None)
            .Returns(call => _fileSystem.Path.Combine(pathValid.FullName, $"test{call.Arg<DownloadMigrationOptions>().Id}.zip"));
        
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        
        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_TryStartMigrationWithTypeButNoRepositories_NoStart()
    {
        var runner = CreateRunner();

        var user = new User("test", "test");

        _credentialStore.LoadTokenAsync(CancellationToken.None).Returns("token");
        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);
        _migrationService.GetMigrationsAsync(CancellationToken.None).Returns(new List<Migration>());

        _ansiConsole.Input.PushCharacter('y');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushCharacter('y');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushCharacter('y');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushKey(ConsoleKey.DownArrow);
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        
        _repositoryService.GetRepositoriesAsync(Arg.Is<RepositoryOptions>(o => o.Type == RepositoryType.Public), CancellationToken.None)
            .Returns(new List<Repository>());

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_TryStartMigrationWithAffiliationAndVisibilityButNoRepositories_NoStart()
    {
        var runner = CreateRunner();

        var user = new User("test", "test");

        _credentialStore.LoadTokenAsync(CancellationToken.None).Returns("token");
        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);
        _migrationService.GetMigrationsAsync(CancellationToken.None).Returns(new List<Migration>());

        _ansiConsole.Input.PushCharacter('y');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushCharacter('y');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushKey(ConsoleKey.DownArrow);
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushKey(ConsoleKey.DownArrow);
        _ansiConsole.Input.PushKey(ConsoleKey.DownArrow);
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        
        _repositoryService.GetRepositoriesAsync(Arg
                .Is<RepositoryOptions>(o => o.Type == null 
                                            && o.Affiliation == RepositoryAffiliation.Collaborator
                                            && o.Visibility == RepositoryVisibility.Private), CancellationToken.None)
            .Returns(new List<Repository>());

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_StartMigrationWithSelectedRepositories_Start()
    {
        var runner = CreateRunner();

        var user = new User("test", "test");

        _credentialStore.LoadTokenAsync(CancellationToken.None).Returns("token");
        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);
        _migrationService.GetMigrationsAsync(CancellationToken.None).Returns(new List<Migration>());

        _ansiConsole.Input.PushCharacter('y');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushCharacter('y');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushKey(ConsoleKey.DownArrow);
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushKey(ConsoleKey.DownArrow);
        _ansiConsole.Input.PushKey(ConsoleKey.DownArrow);
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        
        _repositoryService.GetRepositoriesAsync(Arg
                .Is<RepositoryOptions>(o => o.Type == null 
                                            && o.Affiliation == RepositoryAffiliation.Collaborator
                                            && o.Visibility == RepositoryVisibility.Private), CancellationToken.None)
            .Returns(new List<Repository>
            {
                new("test1"),
                new("test2")
            });
        
        _ansiConsole.Input.PushKey(ConsoleKey.DownArrow);
        _ansiConsole.Input.PushKey(ConsoleKey.Spacebar);
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        
        _ansiConsole.Input.PushKey(ConsoleKey.DownArrow);
        _ansiConsole.Input.PushKey(ConsoleKey.Spacebar);
        _ansiConsole.Input.PushKey(ConsoleKey.DownArrow);
        _ansiConsole.Input.PushKey(ConsoleKey.Spacebar);
        _ansiConsole.Input.PushKey(ConsoleKey.DownArrow);
        _ansiConsole.Input.PushKey(ConsoleKey.DownArrow);
        _ansiConsole.Input.PushKey(ConsoleKey.Spacebar);
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

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