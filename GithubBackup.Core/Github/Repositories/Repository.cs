using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github.Repositories;

public sealed class Repository(string fullName)
{
    [JsonPropertyName("full_name")]
    public string FullName { get; } = fullName;
}
