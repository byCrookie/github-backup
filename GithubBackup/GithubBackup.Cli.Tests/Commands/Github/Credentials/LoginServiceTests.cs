using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Credentials;
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

        var action = () => CreateLoginService().ValidateLoginAsync(CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();

        await Verify(_ansiConsole.Output);
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

        var action = () => CreateLoginService().ValidateLoginAsync(CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();
        
        await Verify(_ansiConsole.Output);
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

        var result = await CreateLoginService().ValidateLoginAsync(CancellationToken.None);

        result.Should().Be(user);

        _logger.VerifyLogs();
        
        await Verify(_ansiConsole.Output);
    }
    
    private LoginService CreateLoginService()
    {
        return new LoginService(_logger, _credentialStore, _userService);
    }
}