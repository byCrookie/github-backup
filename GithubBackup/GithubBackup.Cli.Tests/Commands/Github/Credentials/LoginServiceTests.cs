using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Users;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Spectre.Console.Testing;

namespace GithubBackup.Cli.Tests.Commands.Github.Credentials;

[UsesVerify]
public class LoginServiceTests
{
    private readonly ILogger<LoginService> _logger = Substitute.For<ILogger<LoginService>>();
    private readonly ICredentialStore _credentialStore = Substitute.For<ICredentialStore>();
    private readonly IUserService _userService = Substitute.For<IUserService>();
    private readonly TestConsole _ansiConsole = new();

    [Fact]
    public async Task ValidateLoginAsync_TokenIsInvalid_ThrowException()
    {
        _credentialStore
            .LoadTokenAsync(CancellationToken.None)
            .Returns(string.Empty);

        var action = () => CreateLoginService(true, false).ValidateLoginAsync(CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "No token found")
        );

        _ansiConsole.Output.Should().BeEmpty();
    }
    
    [Fact]
    public async Task ValidateLoginAsync_WhoAmIAsyncFailed_ThrowException()
    {
        _credentialStore
            .LoadTokenAsync(CancellationToken.None)
            .Returns("token");
        
        _userService
            .WhoAmIAsync(CancellationToken.None)
            .ThrowsAsync(new Exception("test"));

        var action = () => CreateLoginService(true, false).ValidateLoginAsync(CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "Token is invalid")
        );
        
        _ansiConsole.Output.Should().BeEmpty();
    }
    
    [Fact]
    public async Task ValidateLoginAsync_WhoAmIAsync_ReturnUser()
    {
        _credentialStore
            .LoadTokenAsync(CancellationToken.None)
            .Returns("token");
        
        var user = new User("test", "test");
        
        _userService
            .WhoAmIAsync(CancellationToken.None)
            .Returns(user);

        var result = await CreateLoginService(true, false).ValidateLoginAsync(CancellationToken.None);

        result.Should().Be(user);

        _logger.VerifyLogs();
        
        _ansiConsole.Output.Should().BeEmpty();
    }
    
    [Fact]
    public async Task ValidateLoginAsync_WhoAmIAsyncAndInteractivePromptForReloginIsTrue_ReturnUser()
    {
        _credentialStore
            .LoadTokenAsync(CancellationToken.None)
            .Returns("token");
        
        var user = new User("test", "test");
        
        _userService
            .WhoAmIAsync(CancellationToken.None)
            .Returns(user);
        
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        var result = await CreateLoginService(true, true).ValidateLoginAsync(CancellationToken.None);

        result.Should().Be(user);

        _logger.VerifyLogs();
        
        Verify(_ansiConsole.Output);
    }
    
    [Fact]
    public async Task ValidateLoginAsync_WhoAmIAsyncAndInteractivePromptForReloginIsFalse_ThrowException()
    {
        _credentialStore
            .LoadTokenAsync(CancellationToken.None)
            .Returns("token");
        
        var user = new User("test", "test");
        
        _userService
            .WhoAmIAsync(CancellationToken.None)
            .Returns(user);
        
        _ansiConsole.Input.PushCharacter('n');
        _ansiConsole.Input.PushKey(ConsoleKey.Enter);

        var action = () => CreateLoginService(true, true).ValidateLoginAsync(CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();

        _logger.VerifyLogs();
        
        Verify(_ansiConsole.Output);
    }
    
    private LoginService CreateLoginService(bool quiet, bool interactive)
    {
        var globalArgs = new GlobalArgs(LogLevel.Debug, quiet, new FileInfo("test"), interactive);
        return new LoginService(globalArgs, _logger, _credentialStore, _userService, _ansiConsole);
    }
}