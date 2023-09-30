using Flurl.Http;
using Microsoft.Net.Http.Headers;
using Polly;

namespace GithubBackup.Core.Github;

internal class GithubService : IGithubService
{
    private const string ClientId = "e197b2a7e36e8a0d5ea9";

    public Task<DeviceAndUserCodesResponse> RequestDeviceAndUserCodesAsync(CancellationToken ct)
    {
        const string scope = "repo,read:user,read:org";

        return "https://github.com/login/device/code"
            .WithHeader(HeaderNames.Accept, "application/json")
            .PostJsonAsync(new { client_id = ClientId, scope }, ct)
            .ReceiveJson<DeviceAndUserCodesResponse>();
    }

    public Task<AccessTokenResponse> PollForAccessTokenAsync(string deviceCode, int interval, CancellationToken ct)
    {
        const string grantType = "urn:ietf:params:oauth:grant-type:device_code";

        var currentInterval = new IntervalWrapper(TimeSpan.FromSeconds(interval));

        var policy = Policy
            .HandleResult<AccessTokenResponse>(response => !string.IsNullOrWhiteSpace(response.Error))
            .RetryForeverAsync(response => OnRetryAsync(response.Result, currentInterval, ct));

        return policy.ExecuteAsync(() => "https://github.com/login/oauth/access_token"
            .WithHeader(HeaderNames.Accept, "application/json")
            .PostJsonAsync(new { client_id = ClientId, device_code = deviceCode, grant_type = grantType }, ct)
            .ReceiveJson<AccessTokenResponse>());
    }

    private static async Task OnRetryAsync(AccessTokenResponse response, IntervalWrapper intervalWrapper, CancellationToken ct)
    {
        switch (response.Error)
        {
            case "authorization_pending":
            {
                var delay = intervalWrapper.Interval;
                Console.WriteLine($"Authorization pending. Retrying in {delay.TotalSeconds} seconds.");
                await Task.Delay(delay, ct);
                return;
            }
            case "slow_down":
            {
                var newDelay = TimeSpan.FromSeconds(response.Interval ?? intervalWrapper.Interval.TotalSeconds + 5);
                intervalWrapper.Update(newDelay);
                Console.WriteLine($"Slow down. Retrying in {newDelay.TotalSeconds} seconds.");
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