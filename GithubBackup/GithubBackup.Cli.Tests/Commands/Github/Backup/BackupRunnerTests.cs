using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Github.Backup;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Migrations;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console.Testing;

namespace GithubBackup.Cli.Tests.Commands.Github.Backup;

[UsesVerify]
public class BackupRunnerTests
{
    private readonly IMigrationService _migrationService = Substitute.For<IMigrationService>();
    private readonly ILoginService _loginService = Substitute.For<ILoginService>();
    private readonly IFileSystem _fileSystem = new MockFileSystem();
    private readonly TestConsole _ansiConsole = new();

    [Fact]
    public async Task RunAsync_Quiet_DoNotWriteToConsole()
    {
        var backupRunner = CreateBackupRunner(true);

        const int id = 1;
        
        _migrationService
            .StartMigrationAsync(Arg.Any<StartMigrationOptions>(), CancellationToken.None)
            .Returns(new Migration(id, MigrationState.Pending, new DateTime(2020, 1, 1)));

        const string migrationFile = "test";

        _migrationService
            .PollAndDownloadMigrationAsync(Arg.Is<DownloadMigrationOptions>(o => o.Id == id), Arg.Any<Func<Migration, Task>>(), Arg.Any<CancellationToken>())
            .Returns(migrationFile);

        await backupRunner.RunAsync(CancellationToken.None);
        
        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_NotQuiet_DoWriteToConsole()
    {
        var backupRunner = CreateBackupRunner(false);

        const int id = 1;
        
        _migrationService
            .StartMigrationAsync(Arg.Any<StartMigrationOptions>(), CancellationToken.None)
            .Returns(new Migration(id, MigrationState.Pending, new DateTime(2020, 1, 1)));

        const string migrationFile = "test";

        _migrationService
            .PollAndDownloadMigrationAsync(Arg.Is<DownloadMigrationOptions>(o => o.Id == id), Arg.Any<Func<Migration, Task>>(), Arg.Any<CancellationToken>())
            .Returns(migrationFile);

        await backupRunner.RunAsync(CancellationToken.None);

        await Verify(_ansiConsole.Output);
    }

    private BackupRunner CreateBackupRunner(bool quiet)
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, quiet, new FileInfo("test"));
        var migrateArgs = new MigrateArgs(new []{"test"}, false, false, false, false, false, false, false, new IntervalArgs(null), new LoginArgs(null, false));
        var downloadArgs = new DownloadArgs(Array.Empty<long>(), false, new DirectoryInfo("test"), null, true, new IntervalArgs(null), new LoginArgs(null, false));
        var backupArgs = new BackupArgs(migrateArgs, downloadArgs, new IntervalArgs(null), new LoginArgs(null, false));

        return new BackupRunner(
            globalArgs,
            backupArgs,
            _migrationService,
            _loginService,
            _fileSystem,
            _ansiConsole,
            Substitute.For<IGithubTokenStore>()
        );
    }
}