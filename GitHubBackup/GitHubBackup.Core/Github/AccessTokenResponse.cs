using System.Text.Json.Serialization;

namespace GitHubBackup.Core.Github;

public class AccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
    
    [JsonPropertyName("error")]
    public string? Error { get; set; }
    
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }
    
    [JsonPropertyName("error_uri")]
    public Uri? ErrorUri { get; set; }
    
    [JsonPropertyName("interval")]
    public int? Interval { get; set; }
}