using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Github.Interval;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Users;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console.Testing;

namespace GithubBackup.Cli.Tests.Commands.Github.Migrate;

[UsesVerify]
public class MigrateRunnerTests
{
    private readonly TestConsole _ansiConsole = new();
    private readonly ILogger<LoginRunner> _logger = Substitute.For<ILogger<LoginRunner>>();
    private readonly IMigrationService _migrationService = Substitute.For<IMigrationService>();
    private readonly ILoginService _loginService = Substitute.For<ILoginService>();

    [Fact]
    public async Task RunAsync_Quiet_DoNotWriteToConsoleAndMigrate()
    {
        var runner = CreateRunner(true);

        var user = new User("test", "test");

        _loginService.ValidateLoginAsync(CancellationToken.None).Returns(user);
        
        _migrationService.StartMigrationAsync(Arg.Any<StartMigrationOptions>(), CancellationToken.None)
            .Returns(new Migration(1, MigrationState.Pending, DateTime.UtcNow));

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_NotQuiet_DoWriteToConsoleAndMigrate()
    {
        var runner = CreateRunner(false);

        var user = new User("test", "test");

        _loginService.ValidateLoginAsync(CancellationToken.None).Returns(user);
        
        _migrationService.StartMigrationAsync(Arg.Any<StartMigrationOptions>(), CancellationToken.None)
            .Returns(new Migration(1, MigrationState.Pending, DateTime.UtcNow));

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }

    private MigrateRunner CreateRunner(bool quiet)
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, quiet, new FileInfo("test"));
        var migrateArgs = new MigrateArgs(
            new[] {"test"},
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            new IntervalArgs(null)
        );

        return new MigrateRunner(
            globalArgs,
            migrateArgs,
            _migrationService,
            _loginService,
            _ansiConsole
        );
    }
}