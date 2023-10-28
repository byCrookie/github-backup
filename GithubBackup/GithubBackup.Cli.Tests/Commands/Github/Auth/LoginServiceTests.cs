using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Users;
using GithubBackup.TestUtils.Configuration;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console.Testing;

namespace GithubBackup.Cli.Tests.Commands.Github.Credentials;

[UsesVerify]
public class LoginServiceTests
{
    private readonly TestConsole _ansiConsole = new();
    private readonly ILogger<LoginService> _logger = Substitute.For<ILogger<LoginService>>();
    private readonly IAuthenticationService _authenticationService = Substitute.For<IAuthenticationService>();
    private readonly IUserService _userService = Substitute.For<IUserService>();
    private readonly IGithubTokenStore _githubTokenStore = Substitute.For<IGithubTokenStore>();

    [Fact]
    public async Task RunAsync_QuietAndToken_DoNotWriteToConsoleAndLoginUsingToken()
    {
        const string token = "token";
        var setup = CreateLoginService(true, token, true, null);
        var onToken = string.Empty;
        
        var user = new User("test", "test");

        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        await setup.sut.LoginAsync(setup.globalArgs, setup.loginArgs, (t, _) =>
        {
            onToken = t;
            return Task.CompletedTask;
        }, CancellationToken.None);
        
        onToken.Should().Be(token);
        
        await _githubTokenStore.Received(1).SetAsync(token);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using token from command line")
        );

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_NotQuietAndToken_DoWriteToConsoleAndLoginUsingToken()
    {
        const string token = "token";
        var setup = CreateLoginService(false, token, true, null);
        var onToken = string.Empty;

        var user = new User("test", "test");

        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        await setup.sut.LoginAsync(setup.globalArgs, setup.loginArgs, (t, _) =>
        {
            onToken = t;
            return Task.CompletedTask;
        }, CancellationToken.None);
        
        onToken.Should().Be(token);
        
        await _githubTokenStore.Received(1).SetAsync(token);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using token from command line")
        );

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_QuietAndDeviceAuthFlow_DoNotWriteToConsoleAndLoginUsingDeviceAuthFlow()
    {
        const string token = "token";
        var setup = CreateLoginService(true, null, true, null);
        var onToken = string.Empty;

        const string deviceCode = "deviceCode";
        const int interval = 60;
        var deviceAndUserCodes = new DeviceAndUserCodes(
            deviceCode,
            "userCode",
            "verificationUri",
            60,
            interval
        );

        _authenticationService.RequestDeviceAndUserCodesAsync(CancellationToken.None)
            .Returns(deviceAndUserCodes);

        var accessToken = new AccessToken(token, "type", "scope");
        _authenticationService.PollForAccessTokenAsync(deviceCode, interval, CancellationToken.None)
            .Returns(accessToken);

        var user = new User("test", "test");
        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        await setup.sut.LoginAsync(setup.globalArgs, setup.loginArgs, (t, _) =>
        {
            onToken = t;
            return Task.CompletedTask;
        }, CancellationToken.None);
        
        onToken.Should().Be(token);
        
        await _githubTokenStore.Received(1).SetAsync(token);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using device flow authentication")
        );

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_NotQuietAndDeviceAuthFlow_DoWriteToConsoleAndLoginUsingDeviceAuthFlow()
    {
        const string token = "token";
        var setup = CreateLoginService(false, null, true, null);
        var onToken = string.Empty;
        
        const string deviceCode = "deviceCode";
        const int interval = 60;
        var deviceAndUserCodes = new DeviceAndUserCodes(
            deviceCode,
            "userCode",
            "verificationUri",
            60,
            interval
        );

        _authenticationService.RequestDeviceAndUserCodesAsync(CancellationToken.None)
            .Returns(deviceAndUserCodes);
        
        var accessToken = new AccessToken(token, "type", "scope");
        _authenticationService.PollForAccessTokenAsync(deviceCode, interval, CancellationToken.None)
            .Returns(accessToken);

        var user = new User("test", "test");
        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        await setup.sut.LoginAsync(setup.globalArgs, setup.loginArgs, (t, _) =>
        {
            onToken = t;
            return Task.CompletedTask;
        }, CancellationToken.None);
        
        onToken.Should().Be(token);
        
        await _githubTokenStore.Received(1).SetAsync(token);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using device flow authentication")
        );

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_QuietAndTokenEnv_DoNotWriteToConsoleAndLoginUsingTokenEnv()
    {
        const string token = "token";
        var setup = CreateLoginService(true, null, false, token);
        var onToken = string.Empty;

        var user = new User("test", "test");

        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        await setup.sut.LoginAsync(setup.globalArgs, setup.loginArgs, (t, _) =>
        {
            onToken = t;
            return Task.CompletedTask;
        }, CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using token from environment variable")
        );
        
        onToken.Should().BeEmpty();
        
        await _githubTokenStore.Received(1).SetAsync(token);

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_NotQuietAndTokenEnv_DoWriteToConsoleAndLoginUsingTokenEnv()
    {
        const string token = "token";
        var setup = CreateLoginService(false, null, false, token);
        var onToken = string.Empty;

        var user = new User("test", "test");

        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        await setup.sut.LoginAsync(setup.globalArgs, setup.loginArgs, (t, _) =>
        {
            onToken = t;
            return Task.CompletedTask;
        }, CancellationToken.None);
        
        onToken.Should().BeEmpty();
        
        await _githubTokenStore.Received(1).SetAsync(token);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using token from environment variable")
        );

        await Verify(_ansiConsole.Output);
    }

    private (GlobalArgs globalArgs, LoginArgs loginArgs, LoginService sut) CreateLoginService(bool quiet, string? token, bool deviceFlowAuth, string? envToken)
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, quiet, new FileInfo("test"));
        var loginArgs = new LoginArgs(token, deviceFlowAuth);

        var configuration = CreateConfiguration(envToken);

        var sut = new LoginService(
            _logger,
            _userService,
            configuration,
            _authenticationService,
            _ansiConsole,
            _githubTokenStore
        );
        
        return (globalArgs, loginArgs, sut);
    }

    private static IConfigurationRoot CreateConfiguration(string? envToken)
    {
        if (string.IsNullOrWhiteSpace(envToken))
        {
            return new ConfigurationBuilder().Build();
        }

        return new ConfigurationBuilder()
            .Add(new KeyValueConfigurationProvider(new Dictionary<string, string?>
            {
                { "TOKEN", envToken }
            }))
            .Build();
    }
}