using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github.Repositories;

internal sealed class RepositoryResponse
{
    [JsonPropertyName("full_name")]
    public string FullName { get; }
    
    public RepositoryResponse(string fullName)
    {
        FullName = fullName;
    }
}