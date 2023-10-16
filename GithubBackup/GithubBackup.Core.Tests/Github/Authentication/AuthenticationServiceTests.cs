using FluentAssertions;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Clients;
using GithubBackup.Core.Tests.Utils;
using GithubBackup.TestUtils.Flurl;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GithubBackup.Core.Tests.Github.Authentication;

public class AuthenticationServiceTests
{
    private readonly AuthenticationService _sut;

    private readonly IGithubWebClient _githubWebClient;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationServiceTests()
    {
        _githubWebClient = Substitute.For<IGithubWebClient>();
        _logger = Substitute.For<ILogger<AuthenticationService>>();

        _sut = new AuthenticationService(_logger, _githubWebClient);
    }

    [Fact]
    public async Task RequestDeviceAndUserCodesAsync_Codes_Return()
    {
        const string deviceCode = "deviceCode";
        const string userCode = "userCode";
        const string verificationUri = "verificationUri";
        const int expiresIn = 1;
        const int interval = 1;

        var reponse = new DeviceAndUserCodesResponse(
            deviceCode,
            userCode,
            verificationUri,
            expiresIn,
            interval
        ).ToFlurlJsonResponse();

        _githubWebClient
            .PostJsonAsync("/login/device/code", Arg.Any<object>(), null, Arg.Any<CancellationToken>())
            .Returns(reponse);

        var result = await _sut.RequestDeviceAndUserCodesAsync(CancellationToken.None);

        result.Should().BeEquivalentTo(new DeviceAndUserCodes(
            deviceCode,
            userCode,
            verificationUri,
            expiresIn,
            interval)
        );

        _logger.VerifyLogs(
            (LogLevel.Debug, "Requesting device and user codes")
        );
    }

    [Fact]
    public async Task PollAndDownloadMigrationAsync_IsPendingAndSlowDown_WaitWithIntervalAndExtendIt()
    {
        const string deviceCode = "deviceCode";
        const int interval = 1;
        const string accessToken = "accessToken";
        const string tokenType = "tokenType";
        const string scope = "scope";

        var reponse1 = new AccessTokenResponse
        {
            AccessToken = null,
            TokenType = null,
            Scope = null,
            Error = "authorization_pending",
            ErrorDescription = null,
            Interval = interval
        }.ToFlurlJsonResponse();

        var reponse2 = new AccessTokenResponse
        {
            AccessToken = null,
            TokenType = null,
            Scope = null,
            Error = "slow_down",
            ErrorDescription = null,
            Interval = interval + 1
        }.ToFlurlJsonResponse();

        var reponse3 = new AccessTokenResponse
        {
            AccessToken = accessToken,
            TokenType = tokenType,
            Scope = scope,
            Error = null,
            ErrorDescription = null,
            Interval = null
        }.ToFlurlJsonResponse();

        _githubWebClient
            .PostJsonAsync("/login/oauth/access_token", Arg.Any<object>(), null, Arg.Any<CancellationToken>())
            .Returns(reponse1, reponse2, reponse3);

        var result = await _sut.PollForAccessTokenAsync(deviceCode, interval, CancellationToken.None);

        _logger.VerifyLogs(
            (LogLevel.Debug, "Polling for access token"),
            (LogLevel.Information, "Authorization pending. Retrying in 1 seconds"),
            (LogLevel.Information, "Slow down. Retrying in 2 seconds")
        );

        result.Should().BeEquivalentTo(new AccessToken(accessToken, tokenType, scope));
    }

    [Fact]
    public async Task PollAndDownloadMigrationAsync_ExpiredToken_ThrowException()
    {
        const string deviceCode = "deviceCode";
        const int interval = 1;

        var reponse = new AccessTokenResponse
        {
            AccessToken = null,
            TokenType = null,
            Scope = null,
            Error = "expired_token",
            ErrorDescription = null,
            Interval = null
        }.ToFlurlJsonResponse();

        _githubWebClient
            .PostJsonAsync("/login/oauth/access_token", Arg.Any<object>(), null, Arg.Any<CancellationToken>())
            .Returns(reponse);

        var action = () => _sut.PollForAccessTokenAsync(deviceCode, interval, CancellationToken.None);

        _logger.VerifyLogs();
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task PollAndDownloadMigrationAsync_AccessDenied_ThrowException()
    {
        const string deviceCode = "deviceCode";
        const int interval = 1;

        var reponse = new AccessTokenResponse
        {
            AccessToken = null,
            TokenType = null,
            Scope = null,
            Error = "access_denied",
            ErrorDescription = null,
            Interval = null
        }.ToFlurlJsonResponse();

        _githubWebClient
            .PostJsonAsync("/login/oauth/access_token", Arg.Any<object>(), null, Arg.Any<CancellationToken>())
            .Returns(reponse);

        var action = () => _sut.PollForAccessTokenAsync(deviceCode, interval, CancellationToken.None);

        _logger.VerifyLogs();
        await action.Should().ThrowAsync<Exception>();
    }
}