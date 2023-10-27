using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Users;
using GithubBackup.TestUtils.Configuration;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console.Testing;

namespace GithubBackup.Cli.Tests.Commands.Github.Login;

[UsesVerify]
public class LoginRunnerTests
{
    private readonly TestConsole _ansiConsole = new();
    private readonly ILogger<LoginRunner> _logger = Substitute.For<ILogger<LoginRunner>>();
    private readonly IAuthenticationService _authenticationService = Substitute.For<IAuthenticationService>();
    private readonly IUserService _userService = Substitute.For<IUserService>();
    private readonly ICredentialStore _credentialStore = Substitute.For<ICredentialStore>();

    [Fact]
    public async Task RunAsync_QuietAndToken_DoNotWriteToConsoleAndLoginUsingToken()
    {
        var runner = CreateRunner(true, "token", true, null);

        var user = new User("test", "test");

        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using token from command line")
        );
        
        await _credentialStore
            .Received(1)
            .StoreTokenAsync("token", CancellationToken.None);

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_NotQuietAndToken_DoWriteToConsoleAndLoginUsingToken()
    {
        var runner = CreateRunner(false, "token", true, null);

        var user = new User("test", "test");

        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using token from command line")
        );
        
        await _credentialStore
            .Received(1)
            .StoreTokenAsync("token", CancellationToken.None);

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_QuietAndDeviceAuthFlow_DoNotWriteToConsoleAndLoginUsingDeviceAuthFlow()
    {
        var runner = CreateRunner(true, null, true, null);

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

        const string token = "token";
        var accessToken = new AccessToken(token, "type", "scope");
        _authenticationService.PollForAccessTokenAsync(deviceCode, interval, CancellationToken.None)
            .Returns(accessToken);

        var user = new User("test", "test");
        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        await runner.RunAsync(CancellationToken.None);

        await _credentialStore
            .Received(1)
            .StoreTokenAsync(token, CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using device flow authentication")
        );

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_NotQuietAndDeviceAuthFlow_DoWriteToConsoleAndLoginUsingDeviceAuthFlow()
    {
        var runner = CreateRunner(false, null, true, null);

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

        const string token = "token";
        var accessToken = new AccessToken(token, "type", "scope");
        _authenticationService.PollForAccessTokenAsync(deviceCode, interval, CancellationToken.None)
            .Returns(accessToken);

        var user = new User("test", "test");
        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        await runner.RunAsync(CancellationToken.None);

        await _credentialStore
            .Received(1)
            .StoreTokenAsync(token, CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using device flow authentication")
        );

        await Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task RunAsync_QuietAndTokenEnv_DoNotWriteToConsoleAndLoginUsingTokenEnv()
    {
        var runner = CreateRunner(true, null, false, "token");

        var user = new User("test", "test");

        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using token from environment variable")
        );

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task RunAsync_NotQuietAndTokenEnv_DoWriteToConsoleAndLoginUsingTokenEnv()
    {
        var runner = CreateRunner(false, null, false, "token");

        var user = new User("test", "test");

        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        await runner.RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using token from environment variable")
        );

        await Verify(_ansiConsole.Output);
    }

    private LoginRunner CreateRunner(bool quiet, string? token, bool deviceFlowAuth, string? envToken)
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, quiet, new FileInfo("test"));
        var loginArgs = new LoginArgs(token, deviceFlowAuth);

        var configuration = CreateConfiguration(envToken);

        return new LoginRunner(
            globalArgs,
            loginArgs,
            _authenticationService,
            _credentialStore,
            _userService,
            configuration,
            _logger,
            _ansiConsole
        );
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