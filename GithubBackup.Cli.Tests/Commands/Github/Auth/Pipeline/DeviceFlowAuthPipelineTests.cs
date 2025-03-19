using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Github.Auth.Pipeline;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Users;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Spectre.Console.Testing;

namespace GithubBackup.Cli.Tests.Commands.Github.Auth.Pipeline;

public class DeviceFlowAuthPipelineTests
{
    private readonly DeviceFlowAuthPipeline _sut;

    private readonly ILoginPipeline _next;

    private readonly ILogger<DeviceFlowAuthPipeline> _logger;
    private readonly IGithubTokenStore _githubTokenStore;
    private readonly IPersistentCredentialStore _persistentCredentialStore;
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authenticationService;
    private readonly TestConsole _ansiConsole;

    public DeviceFlowAuthPipelineTests()
    {
        _logger = Substitute.For<ILogger<DeviceFlowAuthPipeline>>();
        _githubTokenStore = Substitute.For<IGithubTokenStore>();
        _persistentCredentialStore = Substitute.For<IPersistentCredentialStore>();
        _userService = Substitute.For<IUserService>();
        _authenticationService = Substitute.For<IAuthenticationService>();
        _ansiConsole = new TestConsole();

        _sut = new DeviceFlowAuthPipeline(
            _logger,
            _githubTokenStore,
            _persistentCredentialStore,
            _userService,
            _authenticationService,
            _ansiConsole
        );

        _next = Substitute.For<ILoginPipeline>();
        _sut.Next = _next;
    }

    [Fact]
    public async Task LoginAsync_NotResponsible_CallNext()
    {
        var ct = CancellationToken.None;
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var loginArgs = new LoginArgs(null, false);

        await _sut.LoginAsync(globalArgs, loginArgs, true, ct);

        await _next.Received(1).LoginAsync(globalArgs, loginArgs, true, ct);

        _logger.VerifyLogs();
    }

    [Fact]
    public async Task LoginAsync_ValidAndPersist_PersistTokenAndReturnUser()
    {
        const string token = "token";
        var ct = CancellationToken.None;
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var loginArgs = new LoginArgs(null, true);
        var user = new User("test", "test");

        var deviceAndUserCodes = new DeviceAndUserCodes("device", "user", "url", 1, 1);
        _authenticationService.RequestDeviceAndUserCodesAsync(ct).Returns(deviceAndUserCodes);
        var accessToken = new AccessToken(token, "type", "scope");
        _authenticationService
            .PollForAccessTokenAsync(deviceAndUserCodes.DeviceCode, deviceAndUserCodes.Interval, ct)
            .Returns(accessToken);
        _userService.WhoAmIAsync(ct).Returns(user);

        await _sut.LoginAsync(globalArgs, loginArgs, true, ct);

        await _githubTokenStore.Received(1).SetAsync(token);
        await _persistentCredentialStore.Received(1).StoreTokenAsync(token, ct);
        await _next
            .Received(0)
            .LoginAsync(
                Arg.Any<GlobalArgs>(),
                Arg.Any<LoginArgs>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            );

        _logger.VerifyLogs(new LogEntry(LogLevel.Information, "Using device flow authentication"));
    }

    [Fact]
    public async Task LoginAsync_ValidAndNotPersist_DoNotPersistTokenAndReturnUser()
    {
        const string token = "token";
        var ct = CancellationToken.None;
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var loginArgs = new LoginArgs(null, true);
        var user = new User("test", "test");

        var deviceAndUserCodes = new DeviceAndUserCodes("device", "user", "url", 1, 1);
        _authenticationService.RequestDeviceAndUserCodesAsync(ct).Returns(deviceAndUserCodes);
        var accessToken = new AccessToken(token, "type", "scope");
        _authenticationService
            .PollForAccessTokenAsync(deviceAndUserCodes.DeviceCode, deviceAndUserCodes.Interval, ct)
            .Returns(accessToken);
        _userService.WhoAmIAsync(ct).Returns(user);

        await _sut.LoginAsync(globalArgs, loginArgs, false, ct);

        await _githubTokenStore.Received(1).SetAsync(token);
        await _persistentCredentialStore
            .Received(0)
            .StoreTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _next
            .Received(0)
            .LoginAsync(
                Arg.Any<GlobalArgs>(),
                Arg.Any<LoginArgs>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            );

        _logger.VerifyLogs(new LogEntry(LogLevel.Information, "Using device flow authentication"));
    }

    [Fact]
    public async Task LoginAsync_Invalid_ThrowException()
    {
        const string token = "token";
        var ct = CancellationToken.None;
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var loginArgs = new LoginArgs(null, true);

        var deviceAndUserCodes = new DeviceAndUserCodes("device", "user", "url", 1, 1);
        _authenticationService.RequestDeviceAndUserCodesAsync(ct).Returns(deviceAndUserCodes);
        var accessToken = new AccessToken(token, "type", "scope");
        _authenticationService
            .PollForAccessTokenAsync(deviceAndUserCodes.DeviceCode, deviceAndUserCodes.Interval, ct)
            .Returns(accessToken);
        _userService.WhoAmIAsync(ct).ThrowsAsync<Exception>();

        var action = () => _sut.LoginAsync(globalArgs, loginArgs, true, ct);

        await action.Should().ThrowAsync<Exception>();

        await _githubTokenStore.Received(1).SetAsync(token);
        await _persistentCredentialStore
            .Received(0)
            .StoreTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using device flow authentication"),
            new LogEntry(LogLevel.Error, """Token \(null\) is invalid""")
        );
    }
}
