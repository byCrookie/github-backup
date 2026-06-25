using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github.Users;

internal sealed record UserResponse(
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("name")] string Name
);
