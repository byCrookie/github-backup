using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github.Authentication;

internal sealed record AccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; init; }

    [JsonPropertyName("scope")]
    public string? Scope { get; init; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; init; }

    [JsonPropertyName("error_uri")]
    public Uri? ErrorUri { get; init; }

    [JsonPropertyName("interval")]
    public int? Interval { get; init; }
}
