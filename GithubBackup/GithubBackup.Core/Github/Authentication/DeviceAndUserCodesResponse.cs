using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github.Authentication;

internal class DeviceAndUserCodesResponse
{
    [JsonPropertyName("device_code")]
    public string DeviceCode { get; }
    
    [JsonPropertyName("user_code")]
    public string UserCode { get; }
    
    [JsonPropertyName("verification_uri")]
    public string VerificationUri { get; }
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; }
    
    [JsonPropertyName("interval")]
    public int Interval { get; }

    public DeviceAndUserCodesResponse(string deviceCode, string userCode, string verificationUri, int expiresIn, int interval)
    {
        DeviceCode = deviceCode;
        UserCode = userCode;
        VerificationUri = verificationUri;
        ExpiresIn = expiresIn;
        Interval = interval;
    }
}