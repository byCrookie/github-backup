using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Repositories;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Repositories;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console.Testing;

namespace GithubBackup.Cli.Tests.Commands.Github.Repositories;

[UsesVerify]
public class RepositoriesRunnerTests
{
    private readonly TestConsole _ansiConsole = new();
    private readonly ILogger<LoginRunner> _logger = Substitute.For<ILogger<LoginRunner>>();
    private readonly IRepositoryService _repositoryService = Substitute.For<IRepositoryService>();
    private readonly ILoginService _loginService = Substitute.For<ILoginService>();

    [Fact]
    public async Task RunAsync_QuietAndNoRepositories_DoNotWriteToConsoleAndReadRepositories()
    {
        var runner = CreateRunner(true);
        
        _repositoryService.GetRepositoriesAsync(Arg.Any<RepositoryOptions>(), CancellationToken.None)
            .Returns(new List<Repository>());

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_NotQuietAndNoRepositories_DoWriteToConsoleAndReadRepositories()
    {
        var runner = CreateRunner(false);
        
        _repositoryService.GetRepositoriesAsync(Arg.Any<RepositoryOptions>(), CancellationToken.None)
            .Returns(new List<Repository>());

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_QuietAndRepositories_DoNotWriteToConsoleAndReadRepositories()
    {
        var runner = CreateRunner(true);

        var repositories = new List<Repository>
        {
            new("test"),
            new("test2")
        };
        
        _repositoryService.GetRepositoriesAsync(Arg.Any<RepositoryOptions>(), CancellationToken.None)
            .Returns(repositories);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_NotQuietAndRepositories_DoWriteToConsoleAndReadRepositories()
    {
        var runner = CreateRunner(false);
        
        var repositories = new List<Repository>
        {
            new("test"),
            new("test2")
        };
        
        _repositoryService.GetRepositoriesAsync(Arg.Any<RepositoryOptions>(), CancellationToken.None)
            .Returns(repositories);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs();

        await Verify(_ansiConsole.Output);
    }

    private RepositoriesRunner CreateRunner(bool quiet)
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, quiet, new FileInfo("test"));
        var migrateArgs = new RepositoriesArgs(
            RepositoryType.Public,
            RepositoryAffiliation.Owner,
            RepositoryVisibility.All,
            new LoginArgs(null, false)
        );

        return new RepositoriesRunner(
            globalArgs,
            migrateArgs,
            _repositoryService,
            _loginService,
            _ansiConsole
        );
    }
}