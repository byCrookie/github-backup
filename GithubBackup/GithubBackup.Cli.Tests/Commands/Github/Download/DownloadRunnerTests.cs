using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Backup;
using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Users;
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

    public DownloadRunnerTests()
    {
        Piping.IsEnabled = false;
    }
    
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

        await Verify(_ansiConsole.Lines);
    }

    private DownloadRunner CreateRunner(bool quiet, bool latest)
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, quiet, new FileInfo("test"), false);
        var downloadArgs = new DownloadArgs(Array.Empty<long>(), latest, new DirectoryInfo("test"), null, true);

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