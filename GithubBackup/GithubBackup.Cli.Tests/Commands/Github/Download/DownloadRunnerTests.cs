﻿using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Users;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console.Testing;

namespace GithubBackup.Cli.Tests.Commands.Github.Download;

[UsesVerify]
public class DownloadRunnerTests
{
    private readonly IMigrationService _migrationService = Substitute.For<IMigrationService>();
    private readonly ILoginService _loginService = Substitute.For<ILoginService>();
    private readonly IFileSystem _fileSystem = new MockFileSystem();
    private readonly TestConsole _ansiConsole = new();
    private readonly ILogger<DownloadRunner> _logger = Substitute.For<ILogger<DownloadRunner>>();
    
    [Fact]
    public async Task RunAsync_QuietAndLatest_DoNotWriteToConsoleAndDownloadLatest()
    {
        var runner = CreateRunner(true, true);

        var user = new User("test", "test");

        _loginService.ValidateLoginAsync(CancellationToken.None).Returns(user);

        const int id = 1;

        _migrationService
            .GetMigrationsAsync(CancellationToken.None)
            .Returns(new List<Migration>
            {
                new(0, MigrationState.Failed, new DateTime(2022, 1, 1)),
                new(id, MigrationState.Exported, new DateTime(2021, 1, 1)),
                new(2, MigrationState.Pending, new DateTime(2020, 1, 1))
            });

        _migrationService
            .DownloadMigrationAsync(Arg.Is<DownloadMigrationOptions>(o => o.Id == id), CancellationToken.None)
            .Returns("test");

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Downloading latest migration"),
            new LogEntry(LogLevel.Information, "Downloading migration 1 to test"),
            new LogEntry(LogLevel.Information, "Downloaded migration 1 to test")
        );

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_NotQuietAndLatest_DoWriteToConsoleAndDownloadLatest()
    {
        var runner = CreateRunner(false, true);

        var user = new User("test", "test");

        _loginService.ValidateLoginAsync(CancellationToken.None).Returns(user);

        const int id = 1;

        _migrationService
            .GetMigrationsAsync(CancellationToken.None)
            .Returns(new List<Migration>
            {
                new(0, MigrationState.Failed, new DateTime(2022, 1, 1)),
                new(id, MigrationState.Exported, new DateTime(2021, 1, 1)),
                new(2, MigrationState.Pending, new DateTime(2020, 1, 1))
            });

        _migrationService
            .DownloadMigrationAsync(Arg.Is<DownloadMigrationOptions>(o => o.Id == id), CancellationToken.None)
            .Returns("test");

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Downloading latest migration"),
            new LogEntry(LogLevel.Information, "Downloading migration 1 to test"),
            new LogEntry(LogLevel.Information, "Downloaded migration 1 to test")
        );

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_QuietAndNoMigrations_DoNotWriteToConsoleAndDownloadLatest()
    {
        var runner = CreateRunner(true, false);

        var user = new User("test", "test");

        _loginService.ValidateLoginAsync(CancellationToken.None).Returns(user);

        const int id = 1;

        _migrationService
            .GetMigrationsAsync(CancellationToken.None)
            .Returns(new List<Migration>
            {
                new(0, MigrationState.Failed, new DateTime(2022, 1, 1)),
                new(id, MigrationState.Exported, new DateTime(2021, 1, 1)),
                new(2, MigrationState.Pending, new DateTime(2020, 1, 1))
            });

        _migrationService
            .DownloadMigrationAsync(Arg.Is<DownloadMigrationOptions>(o => o.Id == id), CancellationToken.None)
            .Returns("test");

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "No migration ids specified, downloading latest migration"),
            new LogEntry(LogLevel.Information, "Downloading migration 1 to test"),
            new LogEntry(LogLevel.Information, "Downloaded migration 1 to test")
        );

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_NotQuietAndLatestAndNoMigrations_DoWriteToConsoleAndDownloadLatest()
    {
        var runner = CreateRunner(false, false);

        var user = new User("test", "test");

        _loginService.ValidateLoginAsync(CancellationToken.None).Returns(user);

        const int id = 1;

        _migrationService
            .GetMigrationsAsync(CancellationToken.None)
            .Returns(new List<Migration>
            {
                new(0, MigrationState.Failed, new DateTime(2022, 1, 1)),
                new(id, MigrationState.Exported, new DateTime(2021, 1, 1)),
                new(2, MigrationState.Pending, new DateTime(2020, 1, 1))
            });

        _migrationService
            .DownloadMigrationAsync(Arg.Is<DownloadMigrationOptions>(o => o.Id == id), CancellationToken.None)
            .Returns("test");

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "No migration ids specified, downloading latest migration"),
            new LogEntry(LogLevel.Information, "Downloading migration 1 to test"),
            new LogEntry(LogLevel.Information, "Downloaded migration 1 to test")
        );

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_QuietAndLatestAndNoExportedMigrations_DoNotWriteToConsoleAndDoNotDownload()
    {
        var runner = CreateRunner(true, true);

        var user = new User("test", "test");

        _loginService.ValidateLoginAsync(CancellationToken.None).Returns(user);

        _migrationService
            .GetMigrationsAsync(CancellationToken.None)
            .Returns(new List<Migration>
            {
                new(0, MigrationState.Failed, new DateTime(2022, 1, 1)),
                new(1, MigrationState.Exporting, new DateTime(2021, 1, 1)),
                new(2, MigrationState.Pending, new DateTime(2020, 1, 1))
            });

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Downloading latest migration"),
            new LogEntry(LogLevel.Information, "No exported migrations found")
        );

        await Verify(_ansiConsole.Output);

        await _migrationService
            .DidNotReceiveWithAnyArgs()
            .DownloadMigrationAsync(Arg.Any<DownloadMigrationOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_NotQuietAndLatestAndNoExportedMigrations_DoWriteToConsoleAndDoNotDownload()
    {
        var runner = CreateRunner(false, true);

        var user = new User("test", "test");

        _loginService.ValidateLoginAsync(CancellationToken.None).Returns(user);

        _migrationService
            .GetMigrationsAsync(CancellationToken.None)
            .Returns(new List<Migration>
            {
                new(0, MigrationState.Failed, new DateTime(2022, 1, 1)),
                new(1, MigrationState.Exporting, new DateTime(2021, 1, 1)),
                new(2, MigrationState.Pending, new DateTime(2020, 1, 1))
            });

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Downloading latest migration"),
            new LogEntry(LogLevel.Information, "No exported migrations found")
        );
        
        await _migrationService
            .DidNotReceiveWithAnyArgs()
            .DownloadMigrationAsync(Arg.Any<DownloadMigrationOptions>(), Arg.Any<CancellationToken>());

        await Verify(_ansiConsole.Output);
    }

        [Fact]
    public async Task RunAsync_QuietAndExportMigrations_DoNotWriteToConsoleAndDoDownload()
    {
        var runner = CreateRunner(true, true, new[] { 1L, 2L });

        var user = new User("test", "test");

        _loginService.ValidateLoginAsync(CancellationToken.None).Returns(user);

        _migrationService
            .GetMigrationsAsync(CancellationToken.None)
            .Returns(new List<Migration>
            {
                new(0, MigrationState.Pending, new DateTime(2022, 1, 1)),
                new(1, MigrationState.Exported, new DateTime(2021, 1, 1)),
                new(2, MigrationState.Exported, new DateTime(2020, 1, 1))
            });
        
        _migrationService
            .DownloadMigrationAsync(Arg.Is<DownloadMigrationOptions>(o => o.Id == 1), CancellationToken.None)
            .Returns("test1");
        
        _migrationService
            .DownloadMigrationAsync(Arg.Is<DownloadMigrationOptions>(o => o.Id == 2), CancellationToken.None)
            .Returns("test2");

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Downloading migrations using ids"),
            new LogEntry(LogLevel.Information, "Downloading migration 1 to test"),
            new LogEntry(LogLevel.Information, "Downloaded migration 1 to test1"),
            new LogEntry(LogLevel.Information, "Downloading migration 2 to test"),
            new LogEntry(LogLevel.Information, "Downloaded migration 2 to test2")
        );

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_NotQuietAndExportMigrations_DoWriteToConsoleAndDoDownload()
    {
        var runner = CreateRunner(false, true, new[] { 1L, 2L });

        var user = new User("test", "test");

        _loginService.ValidateLoginAsync(CancellationToken.None).Returns(user);

        _migrationService
            .GetMigrationsAsync(CancellationToken.None)
            .Returns(new List<Migration>
            {
                new(0, MigrationState.Failed, new DateTime(2022, 1, 1)),
                new(1, MigrationState.Exported, new DateTime(2021, 1, 1)),
                new(2, MigrationState.Exported, new DateTime(2020, 1, 1))
            });
        
        _migrationService
            .DownloadMigrationAsync(Arg.Is<DownloadMigrationOptions>(o => o.Id == 1), CancellationToken.None)
            .Returns("test1");
        
        _migrationService
            .DownloadMigrationAsync(Arg.Is<DownloadMigrationOptions>(o => o.Id == 2), CancellationToken.None)
            .Returns("test2");

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Downloading migrations using ids"),
            new LogEntry(LogLevel.Information, "Downloading migration 1 to test"),
            new LogEntry(LogLevel.Information, "Downloaded migration 1 to test1"),
            new LogEntry(LogLevel.Information, "Downloading migration 2 to test"),
            new LogEntry(LogLevel.Information, "Downloaded migration 2 to test2")
        );

        await Verify(_ansiConsole.Output);
    }

    private DownloadRunner CreateRunner(bool quiet, bool latest, long[]? ids = null)
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, quiet, new FileInfo("test"));
        var downloadArgs = new DownloadArgs(ids ?? Array.Empty<long>(), latest, new DirectoryInfo("test"), null, true, new IntervalArgs(null));

        return new DownloadRunner(
            globalArgs,
            downloadArgs,
            _migrationService,
            _loginService,
            _fileSystem,
            _logger,
            _ansiConsole
        );
    }
}