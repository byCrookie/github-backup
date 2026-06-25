using AwesomeAssertions;
using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Users;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console.Testing;

namespace GithubBackup.Cli.Tests.Commands.Github.Auth;

public class LoginServiceTests
{
    private readonly LoginService _sut;
    private readonly IGithubTokenStore _githubTokenStore;
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authenticationService;
    private readonly ITemporaryCredentialStore _temporaryCredentialStore;
    private readonly IDateTimeOffsetProvider _dateTimeOffsetProvider;
    private readonly TestConsole _ansiConsole;

    private static readonly GlobalArgs GlobalArgs = new(
        LogLevel.Debug,
        false,
        new FileInfo("Test")
    );

    public LoginServiceTests()
    {
        _githubTokenStore = Substitute.For<IGithubTokenStore>();
        _userService = Substitute.For<IUserService>();
        _authenticationService = Substitute.For<IAuthenticationService>();
        _temporaryCredentialStore = Substitute.For<ITemporaryCredentialStore>();
        _dateTimeOffsetProvider = Substitute.For<IDateTimeOffsetProvider>();
        _ansiConsole = new TestConsole();

        _dateTimeOffsetProvider.UtcNow.Returns(
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        );

        _sut = new LoginService(
            Substitute.For<ILogger<LoginService>>(),
            _ansiConsole,
            CreateConfiguration(null),
            _githubTokenStore,
            _userService,
            _authenticationService,
            _temporaryCredentialStore,
            _dateTimeOffsetProvider
        );
    }

    [Fact]
    public async Task LoginAsync_TokenArgument_UsesTokenArgumentFirst()
    {
        var user = new User("test", "test");
        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        var result = await _sut.LoginAsync(
            GlobalArgs,
            new LoginArgs("cli-token", false),
            CancellationToken.None
        );

        result.Should().Be(user);
        await _githubTokenStore.Received(1).SetAsync("cli-token");
        await _temporaryCredentialStore
            .DidNotReceive()
            .LoadTokenAsync(Arg.Any<CancellationToken>());
        await _authenticationService
            .DidNotReceive()
            .RequestDeviceAndUserCodesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_EnvironmentToken_UsesEnvironmentBeforeCache()
    {
        var sut = CreateSutWithToken("env-token");
        var user = new User("test", "test");
        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        var result = await sut.LoginAsync(
            GlobalArgs,
            new LoginArgs(null, false),
            CancellationToken.None
        );

        result.Should().Be(user);
        await _githubTokenStore.Received(1).SetAsync("env-token");
        await _temporaryCredentialStore
            .DidNotReceive()
            .LoadTokenAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_TemporaryTokenIsValid_UsesTemporaryTokenBeforeDeviceFlow()
    {
        var user = new User("test", "test");
        _temporaryCredentialStore
            .LoadTokenAsync(CancellationToken.None)
            .Returns(
                new TemporaryCredential(
                    "cached-token",
                    new DateTimeOffset(2026, 1, 1, 1, 0, 0, TimeSpan.Zero)
                )
            );
        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        var result = await _sut.LoginAsync(
            GlobalArgs,
            new LoginArgs(null, false),
            CancellationToken.None
        );

        result.Should().Be(user);
        await _githubTokenStore.Received(1).SetAsync("cached-token");
        await _authenticationService
            .DidNotReceive()
            .RequestDeviceAndUserCodesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_TemporaryTokenExpired_DeletesTokenAndUsesDeviceFlow()
    {
        var user = new User("test", "test");
        _temporaryCredentialStore
            .LoadTokenAsync(CancellationToken.None)
            .Returns(
                new TemporaryCredential(
                    "cached-token",
                    new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
                )
            );
        _authenticationService
            .RequestDeviceAndUserCodesAsync(CancellationToken.None)
            .Returns(
                new DeviceAndUserCodes("device", "user", "https://github.com/login/device", 900, 5)
            );
        _authenticationService
            .PollForAccessTokenAsync("device", 5, CancellationToken.None)
            .Returns(new AccessToken("device-token", "bearer", string.Empty, 60));
        _userService.WhoAmIAsync(CancellationToken.None).Returns(user);

        var result = await _sut.LoginAsync(
            GlobalArgs,
            new LoginArgs(null, false),
            CancellationToken.None
        );

        result.Should().Be(user);
        await _temporaryCredentialStore.Received(1).DeleteTokenAsync(CancellationToken.None);
        await _githubTokenStore.Received(1).SetAsync("device-token");
        await _temporaryCredentialStore
            .Received(1)
            .StoreTokenAsync(
                "device-token",
                new DateTimeOffset(2026, 1, 1, 0, 1, 0, TimeSpan.Zero),
                CancellationToken.None
            );
    }

    [Fact]
    public async Task TryLoginWithTemporaryTokenAsync_NoTemporaryToken_ReturnsNull()
    {
        _temporaryCredentialStore
            .LoadTokenAsync(CancellationToken.None)
            .Returns((TemporaryCredential?)null);

        var result = await _sut.TryLoginWithTemporaryTokenAsync(
            GlobalArgs,
            new LoginArgs(null, false),
            CancellationToken.None
        );

        result.Should().BeNull();
        await _authenticationService
            .DidNotReceive()
            .RequestDeviceAndUserCodesAsync(Arg.Any<CancellationToken>());
    }

    private LoginService CreateSutWithToken(string? token)
    {
        return new LoginService(
            Substitute.For<ILogger<LoginService>>(),
            _ansiConsole,
            CreateConfiguration(token),
            _githubTokenStore,
            _userService,
            _authenticationService,
            _temporaryCredentialStore,
            _dateTimeOffsetProvider
        );
    }

    private static IConfiguration CreateConfiguration(string? token)
    {
        var values = token is null
            ? new Dictionary<string, string?>()
            : new Dictionary<string, string?> { { "TOKEN", token } };

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
