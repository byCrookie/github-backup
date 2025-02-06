using System.Globalization;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Manual;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Repositories;
using GithubBackup.Core.Github.Users;
using GithubBackup.Core.Utils;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console.Testing;
using Environment = GithubBackup.Core.Environment.Environment;

namespace GithubBackup.Cli.Tests.Commands.Github.Manual;


public class ManualRunnerTests
{
    private readonly TestConsole _ansiConsole = new();
    private readonly ILogger<LoginRunner> _logger = Substitute.For<ILogger<LoginRunner>>();
    private readonly IMigrationService _migrationService = Substitute.For<IMigrationService>();
    private readonly IRepositoryService _repositoryService = Substitute.For<IRepositoryService>();
    private readonly IFileSystem _fileSystem = new MockFileSystem();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly ILoginService _loginService = Substitute.For<ILoginService>();

    public ManualRunnerTests()
    {
        _ansiConsole.Profile.Capabilities.Interactive = true;
        CultureInfo.CurrentCulture = new CultureInfo("de-CH");
    }

    [Fact]
    public async Task RunAsync_HasValidTokenAndWantsToContinue_StopWhenNoRepositories()
    {
        var runner = CreateRunner();

        var user = new User("test", "test");
        
        _migrationService.GetMigrationsAsync(CancellationToken.None).Returns(new List<Migration>());
        _loginService.PersistentOnlyAsync(Arg.Any<GlobalArgs>(), Arg.Any<LoginArgs>(), Arg.Any<CancellationToken>())
            .Returns(user);
        
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
        
        const string validToken = "token1";
        
        _migrationService.GetMigrationsAsync(CancellationToken.None).Returns(new List<Migration>());
        _loginService.PersistentOnlyAsync(Arg.Any<GlobalArgs>(), Arg.Any<LoginArgs>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushText(validToken);
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_HasNoToken_LoginAndStopWhenNoRepositories()
    {
        var runner = CreateRunner();
        
        const string validToken = "token1";
        
        _migrationService.GetMigrationsAsync(CancellationToken.None).Returns(new List<Migration>());
        
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushText(validToken);
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_HasInvalidToken_LoginAndStopWhenNoRepositories()
    {
        var runner = CreateRunner();
        
        const string validToken = "token1";

        _migrationService.GetMigrationsAsync(CancellationToken.None).Returns(new List<Migration>());
        
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushText(validToken);
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_NoMigrations_DoNotBackup()
    {
        var runner = CreateRunner();
        
        var user = new User("test", "test");
        
        _loginService.PersistentOnlyAsync(Arg.Any<GlobalArgs>(), Arg.Any<LoginArgs>(), Arg.Any<CancellationToken>())
            .Returns(user);
        
        _ansiConsole.Input.PushCharacter('y');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

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
        
        _loginService.PersistentOnlyAsync(Arg.Any<GlobalArgs>(), Arg.Any<LoginArgs>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _ansiConsole.Input.PushCharacter('y');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

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
        
        _loginService.PersistentOnlyAsync(Arg.Any<GlobalArgs>(), Arg.Any<LoginArgs>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _ansiConsole.Input.PushCharacter('y');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

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

        await Verify(_ansiConsole.Output).UniqueForOSPlatform();
    }

    [Fact]
    public async Task RunAsync_TryStartMigrationWithTypeButNoRepositories_NoStart()
    {
        var runner = CreateRunner();

        var user = new User("test", "test");
        
        _migrationService.GetMigrationsAsync(CancellationToken.None).Returns(new List<Migration>());
        _loginService.PersistentOnlyAsync(Arg.Any<GlobalArgs>(), Arg.Any<LoginArgs>(), Arg.Any<CancellationToken>())
            .Returns(user);

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
        
        _migrationService.GetMigrationsAsync(CancellationToken.None).Returns(new List<Migration>());
        _loginService.PersistentOnlyAsync(Arg.Any<GlobalArgs>(), Arg.Any<LoginArgs>(), Arg.Any<CancellationToken>())
            .Returns(user);

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

        _loginService.PersistentOnlyAsync(Arg.Any<GlobalArgs>(), Arg.Any<LoginArgs>(), Arg.Any<CancellationToken>())
            .Returns(user);
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
            new GlobalArgs(LogLevel.Debug, false, new FileInfo("test")),
            manualBackupArgs,
            _migrationService,
            _repositoryService,
            _fileSystem,
            _ansiConsole,
            _dateTimeProvider,
            _loginService
        );
    }
}