using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Migrations;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Users;
using GithubBackup.Core.Utils;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console.Testing;

namespace GithubBackup.Cli.Tests.Commands.Github.Migrations;

[UsesVerify]
public class MigrationsRunnerTests
{
    private readonly TestConsole _ansiConsole = new();
    private readonly ILogger<LoginRunner> _logger = Substitute.For<ILogger<LoginRunner>>();
    private readonly IMigrationService _migrationService = Substitute.For<IMigrationService>();
    private readonly ILoginService _loginService = Substitute.For<ILoginService>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    [Fact]
    public async Task RunAsync_QuietAndNoMigrations_DoNotWriteToConsoleAndReadMigrations()
    {
        var runner = CreateRunner(true);

        var user = new User("test", "test");

        _loginService.ValidateLoginAsync(CancellationToken.None).Returns(user);
        
        var migrations = new List<Migration>();
        
        _migrationService.GetMigrationsAsync(CancellationToken.None)
            .Returns(migrations);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_NotQuietNoMigrations_DoWriteToConsoleAndReadMigrations()
    {
        var runner = CreateRunner(false);

        var user = new User("test", "test");

        _loginService.ValidateLoginAsync(CancellationToken.None).Returns(user);
        
        var migrations = new List<Migration>();
        
        _migrationService.GetMigrationsAsync(CancellationToken.None)
            .Returns(migrations);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_QuietAndMigrations_DoNotWriteToConsoleAndReadMigrations()
    {
        var runner = CreateRunner(true);

        var user = new User("test", "test");

        _loginService.ValidateLoginAsync(CancellationToken.None).Returns(user);
        
        var migrations = new List<Migration>
        {
            new(1, MigrationState.Pending, new DateTime(2020, 1, 1)),
            new(2, MigrationState.Exporting, new DateTime(2020, 1, 6)),
            new(3, MigrationState.Exported, new DateTime(2020, 1, 10)),
        };
        
        _dateTimeProvider.Now.Returns(new DateTime(2020, 1, 11));
        
        _migrationService.GetMigrationsAsync(CancellationToken.None)
            .Returns(migrations);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_NotQuietAndMigrations_DoWriteToConsoleAndReadMigrations()
    {
        var runner = CreateRunner(false);

        var user = new User("test", "test");

        _loginService.ValidateLoginAsync(CancellationToken.None).Returns(user);
        
        var migrations = new List<Migration>
        {
            new(1, MigrationState.Pending, new DateTime(2020, 1, 1)),
            new(2, MigrationState.Exporting, new DateTime(2020, 1, 6)),
            new(3, MigrationState.Exported, new DateTime(2020, 1, 10)),
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

    private MigrationsRunner CreateRunner(bool quiet)
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, quiet, new FileInfo("test"));
        var migrateArgs = new MigrationsArgs();

        return new MigrationsRunner(
            globalArgs,
            migrateArgs,
            _migrationService,
            _loginService,
            _ansiConsole,
            _dateTimeProvider
        );
    }
}