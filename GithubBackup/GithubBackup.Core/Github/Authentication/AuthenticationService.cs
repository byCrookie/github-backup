﻿using Flurl.Http;
using GithubBackup.Core.Github.Clients;
using Microsoft.Extensions.Logging;
using Polly;

namespace GithubBackup.Core.Github.Authentication;

internal sealed class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IGithubWebClient _githubWebClient;

    private const string ClientId = "e197b2a7e36e8a0d5ea9";

    public AuthenticationService(ILogger<AuthenticationService> logger, IGithubWebClient githubWebClient)
    {
        _logger = logger;
        _githubWebClient = githubWebClient;
    }

    public async Task<DeviceAndUserCodes> RequestDeviceAndUserCodesAsync(CancellationToken ct)
    {
        const string scope = "repo user user:email read:user";

        var response = await _githubWebClient
            .PostJsonAsync("/login/device/code", new { client_id = ClientId, scope }, ct: ct)
            .ReceiveJson<DeviceAndUserCodesResponse>();

        return new DeviceAndUserCodes(
            response.DeviceCode,
            response.UserCode,
            response.VerificationUri,
            response.ExpiresIn,
            response.Interval
        );
    }

    public async Task<AccessToken> PollForAccessTokenAsync(string deviceCode, int interval, CancellationToken ct)
    {
        const string grantType = "urn:ietf:params:oauth:grant-type:device_code";

        var currentInterval = new IntervalWrapper(TimeSpan.FromSeconds(interval));

        var policy = Policy
            .HandleResult<AccessTokenResponse>(response => !string.IsNullOrWhiteSpace(response.Error))
            .RetryForeverAsync(response => OnRetryAsync(response.Result, currentInterval, ct));

        var response = await policy.ExecuteAsync(() => _githubWebClient
            .PostJsonAsync(
                "/login/oauth/access_token",
                new { client_id = ClientId, device_code = deviceCode, grant_type = grantType },
                ct: ct
            )
            .ReceiveJson<AccessTokenResponse>());

        return new AccessToken(response.AccessToken!, response.TokenType!, response.Scope!);
    }

    private async Task OnRetryAsync(AccessTokenResponse response, IntervalWrapper intervalWrapper, CancellationToken ct)
    {
        switch (response.Error)
        {
            case "authorization_pending":
            {
                var delay = intervalWrapper.Interval;
                _logger.LogInformation("Authorization pending. Retrying in {Seconds} seconds", delay.TotalSeconds);
                await Task.Delay(delay, ct);
                return;
            }
            case "slow_down":
            {
                var newDelay = TimeSpan.FromSeconds(response.Interval ?? intervalWrapper.Interval.TotalSeconds + 5);
                intervalWrapper.Update(newDelay);
                _logger.LogInformation("Slow down. Retrying in {Seconds} seconds", newDelay.TotalSeconds);
                await Task.Delay(newDelay, ct);
                return;
            }
            case "expired_token":
            {
                throw new Exception("The device code has expired.");
            }
            case "access_denied":
            {
                throw new Exception("The user has denied the request.");
            }
            default:
            {
                throw new Exception($"Unknown error: {response.Error} - {response.ErrorDescription} - {response.ErrorUri}");
            }
        }
    }
}