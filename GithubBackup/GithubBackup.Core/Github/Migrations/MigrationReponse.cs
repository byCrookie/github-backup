using System.Text.Json.Serialization;
using GithubBackup.Core.Github.Repositories;

namespace GithubBackup.Core.Github.Migrations;

internal class MigrationReponse
{
    [JsonPropertyName("id")]
    public long Id { get; }
    
    [JsonPropertyName("repositories")]
    public List<RepositoryResponse> Repositories { get; }
    
    [JsonPropertyName("state")]
    public MigrationState? State { get; }

    [JsonPropertyName("url")]
    public string Url { get; }

    public MigrationReponse(long id, List<RepositoryResponse> repositories, MigrationState? state, string url)
    {
        Id = id;
        Repositories = repositories;
        State = state;
        Url = url;
    }
}