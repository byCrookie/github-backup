using AwesomeAssertions;
using GithubBackup.Cli.Commands.Github.Auth.Pipeline;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Users;
using GithubBackup.TestUtils.Configuration;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace GithubBackup.Cli.Tests.Commands.Github.Auth.Pipeline;

public class TokenFromConfigurationPipelineTests
{
    private readonly ILogger<TokenFromConfigurationPipeline> _logger = Substitute.For<
        ILogger<TokenFromConfigurationPipeline>
    >();
    private readonly IGithubTokenStore _githubTokenStore = Substitute.For<IGithubTokenStore>();
    private readonly IUserService _userService = Substitute.For<IUserService>();
    private readonly ILoginPipeline _next = Substitute.For<ILoginPipeline>();

    [Fact]
    public async Task LoginAsync_NotResponsible_CallNext()
    {
        var ct = CancellationToken.None;
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var loginArgs = new LoginArgs(null, false);

        await CreatePipeline(null).LoginAsync(globalArgs, loginArgs, true, ct);

        await _next.Received(1).LoginAsync(globalArgs, loginArgs, true, ct);

        _logger.VerifyLogs();
    }

    [Fact]
    public async Task LoginAsync_ValidAndPersist_PersistTokenAndReturnUser()
    {
        const string token = "token";
        var ct = CancellationToken.None;
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var loginArgs = new LoginArgs(token, false);
        var user = new User("test", "test");

        _userService.WhoAmIAsync(ct).Returns(user);

        await CreatePipeline(token).LoginAsync(globalArgs, loginArgs, true, ct);

        await _githubTokenStore.Received(1).SetAsync(token);
        await _next
            .Received(0)
            .LoginAsync(
                Arg.Any<GlobalArgs>(),
                Arg.Any<LoginArgs>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            );

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using token from environment variable")
        );
    }

    [Fact]
    public async Task LoginAsync_ValidAndNotPersist_DoNotPersistTokenAndReturnUser()
    {
        const string token = "token";
        var ct = CancellationToken.None;
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var loginArgs = new LoginArgs(token, false);
        var user = new User("test", "test");

        _userService.WhoAmIAsync(ct).Returns(user);

        await CreatePipeline(token).LoginAsync(globalArgs, loginArgs, false, ct);

        await _githubTokenStore.Received(1).SetAsync(token);
        await _next
            .Received(0)
            .LoginAsync(
                Arg.Any<GlobalArgs>(),
                Arg.Any<LoginArgs>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            );

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using token from environment variable")
        );
    }

    [Fact]
    public async Task LoginAsync_Invalid_ThrowException()
    {
        const string token = "token";
        var ct = CancellationToken.None;
        var globalArgs = new GlobalArgs(LogLevel.Debug, false, new FileInfo("Test"));
        var loginArgs = new LoginArgs(token, false);

        _userService.WhoAmIAsync(ct).ThrowsAsync<Exception>();

        var action = () => CreatePipeline(token).LoginAsync(globalArgs, loginArgs, true, ct);

        await action.Should().ThrowAsync<Exception>();

        await _githubTokenStore.Received(1).SetAsync(token);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Using token from environment variable"),
            new LogEntry(LogLevel.Error, "Token token is invalid")
        );
    }

    private ILoginPipeline CreatePipeline(string? token)
    {
        var configuration = CreateConfiguration(token);

        return new TokenFromConfigurationPipeline(
            _logger,
            configuration,
            _githubTokenStore,
            _userService
        )
        {
            Next = _next,
        };
    }

    private static IConfigurationRoot CreateConfiguration(string? envToken)
    {
        if (string.IsNullOrWhiteSpace(envToken))
        {
            return new ConfigurationBuilder().Build();
        }

        return new ConfigurationBuilder()
            .Add(
                new KeyValueConfigurationProvider(
                    new Dictionary<string, string?> { { "TOKEN", envToken } }
                )
            )
            .Build();
    }
}
