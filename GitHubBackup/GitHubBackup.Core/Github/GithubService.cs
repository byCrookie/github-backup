using Flurl.Http;
using Microsoft.Net.Http.Headers;
using Polly;

namespace GithubBackup.Core.Github;

internal class GithubService : IGithubService
{
    private const string ClientId = "e197b2a7e36e8a0d5ea9";
    private const string AcceptGithubV3Json = "application/vnd.github.v3+json";
    private const string AcceptGithubJson = "application/vnd.github+json";
    private const string AcceptJson = "application/json";
    private const string BaseUrl = "https://github.com";
    private const string ApiBaseUrl = "https://api.github.com";

    public async Task<User> WhoAmIAsync(string accessToken, CancellationToken ct)
    {
        var response = await $"{ApiBaseUrl}/user"
            .WithHeader(HeaderNames.Accept, AcceptGithubJson)
            .WithOAuthBearerToken(accessToken)
            .GetJsonAsync<UserResponse>(ct);

        return new User(response.Login, response.Name, response.Email);
    }

    public async Task<DeviceAndUserCodes> RequestDeviceAndUserCodesAsync(CancellationToken ct)
    {
        const string scope = "repo user user:email read:user";

        var response = await $"{BaseUrl}/login/device/code"
            .WithHeader(HeaderNames.Accept, AcceptJson)
            .PostJsonAsync(new { client_id = ClientId, scope }, ct)
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

        var response = await policy.ExecuteAsync(() => $"{BaseUrl}/login/oauth/access_token"
            .WithHeader(HeaderNames.Accept, AcceptJson)
            .PostJsonAsync(new { client_id = ClientId, device_code = deviceCode, grant_type = grantType }, ct)
            .ReceiveJson<AccessTokenResponse>());

        return new AccessToken(response.AccessToken!, response.TokenType!, response.Scope!);
    }

    public async Task<IReadOnlyCollection<Repository>> GetRepositoriesAsync(string accessToken, CancellationToken ct)
    {
        var response = await $"{ApiBaseUrl}/user/repos"
            .WithHeader(HeaderNames.Accept, AcceptGithubV3Json)
            .WithOAuthBearerToken(accessToken)
            .GetJsonAsync<List<RepositoryResponse>>(ct);

        return new List<Repository>(response.Select(r => new Repository(r.FullName)));
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