using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github;

public class Repository
{
    [JsonPropertyName("full_name")]
    public string FullName { get; }
    
    public Repository(string fullName)
    {
        FullName = fullName;
    }
}