using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Github.Auth.Pipeline;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Users;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace GithubBackup.Cli.Tests.Commands.Github.Auth.Pipeline;

public class PersistedPipelineTests
{
    private readonly PersistedPipeline _sut;

    private readonly ILogger<PersistedPipeline> _logger;
    private readonly IGithubTokenStore _githubTokenStore;
    private readonly IPersistentCredentialStore _persistentCredentialStore;
    private readonly IUserService _userService;

    private readonly ILoginPipeline _next;

    public PersistedPipelineTests()
    {
        _logger = Substitute.For<ILogger<PersistedPipeline>>();
        _githubTokenStore = Substitute.For<IGithubTokenStore>();
        _persistentCredentialStore = Substitute.For<IPersistentCredentialStore>();
        _userService = Substitute.For<IUserService>();

        _sut = new PersistedPipeline(
            _logger,
            _persistentCredentialStore,
            _githubTokenStore,
            _userService
        );

        _next = Substitute.For<ILoginPipeline>();
        _sut.Next = _next;
    }

    [Fact]
    public async Task LoginAsync_NotResponsible_CallNext()
    {
        var ct = CancellationToken.None;
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var loginArgs = new LoginArgs("token", true);

        await _sut.LoginAsync(globalArgs, loginArgs, true, ct);

        await _next.Received(1).LoginAsync(globalArgs, loginArgs, true, ct);

        _logger.VerifyLogs();
    }

    [Fact]
    public async Task LoginAsync_NoToken_CallNext()
    {
        const string? token = null;
        var ct = CancellationToken.None;
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var loginArgs = new LoginArgs(null, false);

        _persistentCredentialStore.LoadTokenAsync(ct).Returns(token);

        await _sut.LoginAsync(globalArgs, loginArgs, true, ct);

        await _githubTokenStore.Received(0).SetAsync(Arg.Any<string>());
        await _persistentCredentialStore.Received(0).StoreTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());

        await _next.Received(1).LoginAsync(globalArgs, loginArgs, true, ct);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using token from persistent store"),
            new LogEntry(LogLevel.Information, "Persistent token not found")
        );
    }

    [Fact]
    public async Task LoginAsync_TokenNotValid_CallNext()
    {
        const string token = "token";
        var ct = CancellationToken.None;
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var loginArgs = new LoginArgs(null, false);

        _persistentCredentialStore.LoadTokenAsync(ct).Returns(token);
        _userService.WhoAmIAsync(ct).ThrowsAsync(new Exception("test"));

        await _sut.LoginAsync(globalArgs, loginArgs, true, ct);

        await _githubTokenStore.Received(0).SetAsync(Arg.Any<string>());
        await _persistentCredentialStore.Received(0).StoreTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());

        await _next.Received(1).LoginAsync(globalArgs, loginArgs, true, ct);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using token from persistent store"),
            new LogEntry(LogLevel.Information, "Persistent token is invalid: test")
        );
    }

    [Fact]
    public async Task LoginAsync_ValidToken_ReturnUser()
    {
        const string token = "token";
        var ct = CancellationToken.None;
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var loginArgs = new LoginArgs(null, false);
        var user = new User("test", "test");

        _userService.WhoAmIAsync(ct).Returns(user);
        _persistentCredentialStore.LoadTokenAsync(ct).Returns(token);

        await _sut.LoginAsync(globalArgs, loginArgs, true, ct);

        await _githubTokenStore.Received(1).SetAsync(token);
        await _persistentCredentialStore.Received(0).StoreTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _next.Received(0).LoginAsync(Arg.Any<GlobalArgs>(), Arg.Any<LoginArgs>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());

        _logger.VerifyLogs(new LogEntry(LogLevel.Information, "Using token from persistent store"));
    }
}