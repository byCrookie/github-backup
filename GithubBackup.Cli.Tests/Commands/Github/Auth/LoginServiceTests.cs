using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Github.Auth.Pipeline;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Users;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console.Testing;

namespace GithubBackup.Cli.Tests.Commands.Github.Auth;

public class LoginServiceTests
{
    private readonly LoginService _sut;
    private readonly ILogger<LoginService> _logger;
    private readonly TestConsole _ansiConsole;
    private readonly ILoginPipelineBuilder _loginPipelineBuilder;

    public LoginServiceTests()
    {
        _logger = Substitute.For<ILogger<LoginService>>();
        _ansiConsole = new TestConsole();
        _loginPipelineBuilder = Substitute.For<ILoginPipelineBuilder>();

        _sut = new LoginService(_logger, _ansiConsole, _loginPipelineBuilder);
    }

    [Fact]
    public async Task PersistentOnlyAsync_NotQuietAndNoUser_ReturnNullAndNotPrint()
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var args = new LoginArgs(null, false);

        var pipeline = Substitute.For<ILoginPipeline>();
        _loginPipelineBuilder.PersistedOnly().Returns(pipeline);
        pipeline.LoginAsync(globalArgs, args, false, CancellationToken.None).Returns((User?)null);

        var result = await _sut.PersistentOnlyAsync(globalArgs, args, CancellationToken.None);

        result.Should().BeNull();

        _logger.VerifyLogs();
        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task PersistentOnlyAsync_NotQuietAndUser_ReturnUserAndPrint()
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var args = new LoginArgs(null, false);
        var user = new User("test", "test");

        var pipeline = Substitute.For<ILoginPipeline>();
        _loginPipelineBuilder.PersistedOnly().Returns(pipeline);
        pipeline.LoginAsync(globalArgs, args, false, CancellationToken.None).Returns(user);

        var result = await _sut.PersistentOnlyAsync(globalArgs, args, CancellationToken.None);

        result.Should().Be(user);

        _logger.VerifyLogs(new LogEntry(LogLevel.Information, "Logged in as test"));
        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task PersistentOnlyAsync_QuietAndUser_ReturnUserAndDoNotPrint()
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, true, new FileInfo("Test"));
        var args = new LoginArgs(null, false);
        var user = new User("test", "test");

        var pipeline = Substitute.For<ILoginPipeline>();
        _loginPipelineBuilder.PersistedOnly().Returns(pipeline);
        pipeline.LoginAsync(globalArgs, args, false, CancellationToken.None).Returns(user);

        var result = await _sut.PersistentOnlyAsync(globalArgs, args, CancellationToken.None);

        result.Should().Be(user);

        _logger.VerifyLogs();
        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task WithoutPersistentAsync_NotQuietAndNoUser_ReturnNullAndNotPrint()
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var args = new LoginArgs(null, false);

        var pipeline = Substitute.For<ILoginPipeline>();
        _loginPipelineBuilder.WithoutPersistent().Returns(pipeline);
        pipeline.LoginAsync(globalArgs, args, false, CancellationToken.None).Returns((User?)null);

        var action = () =>
            _sut.WithoutPersistentAsync(globalArgs, args, false, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();

        _logger.VerifyLogs();
        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task WithoutPersistentAsync_NotQuietAndUser_ReturnUserAndPrint()
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var args = new LoginArgs(null, false);
        var user = new User("test", "test");

        var pipeline = Substitute.For<ILoginPipeline>();
        _loginPipelineBuilder.WithoutPersistent().Returns(pipeline);
        pipeline.LoginAsync(globalArgs, args, false, CancellationToken.None).Returns(user);

        var result = await _sut.WithoutPersistentAsync(
            globalArgs,
            args,
            false,
            CancellationToken.None
        );

        result.Should().Be(user);

        _logger.VerifyLogs(new LogEntry(LogLevel.Information, "Logged in as test"));
        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task WithoutPersistentAsync_QuietAndUser_ReturnUserAndDoNotPrint()
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, true, new FileInfo("Test"));
        var args = new LoginArgs(null, false);
        var user = new User("test", "test");

        var pipeline = Substitute.For<ILoginPipeline>();
        _loginPipelineBuilder.WithoutPersistent().Returns(pipeline);
        pipeline.LoginAsync(globalArgs, args, false, CancellationToken.None).Returns(user);

        var result = await _sut.WithoutPersistentAsync(
            globalArgs,
            args,
            false,
            CancellationToken.None
        );

        result.Should().Be(user);

        _logger.VerifyLogs();
        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task WithPersistentAsync_NotQuietAndNoUser_ReturnNullAndNotPrint()
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var args = new LoginArgs(null, false);

        var pipeline = Substitute.For<ILoginPipeline>();
        _loginPipelineBuilder.WithPersistent().Returns(pipeline);
        pipeline.LoginAsync(globalArgs, args, false, CancellationToken.None).Returns((User?)null);

        var action = () =>
            _sut.WithPersistentAsync(globalArgs, args, false, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();

        _logger.VerifyLogs();
        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task WithPersistentAsync_NotQuietAndUser_ReturnUserAndPrint()
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var args = new LoginArgs(null, false);
        var user = new User("test", "test");

        var pipeline = Substitute.For<ILoginPipeline>();
        _loginPipelineBuilder.WithPersistent().Returns(pipeline);
        pipeline.LoginAsync(globalArgs, args, false, CancellationToken.None).Returns(user);

        var result = await _sut.WithPersistentAsync(
            globalArgs,
            args,
            false,
            CancellationToken.None
        );

        result.Should().Be(user);

        _logger.VerifyLogs(new LogEntry(LogLevel.Information, "Logged in as test"));
        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task WithPersistentAsync_QuietAndUser_ReturnUserAndDoNotPrint()
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, true, new FileInfo("Test"));
        var args = new LoginArgs(null, false);
        var user = new User("test", "test");

        var pipeline = Substitute.For<ILoginPipeline>();
        _loginPipelineBuilder.WithPersistent().Returns(pipeline);
        pipeline.LoginAsync(globalArgs, args, false, CancellationToken.None).Returns(user);

        var result = await _sut.WithPersistentAsync(
            globalArgs,
            args,
            false,
            CancellationToken.None
        );

        result.Should().Be(user);

        _logger.VerifyLogs();
        await Verify(_ansiConsole.Output);
    }
}
