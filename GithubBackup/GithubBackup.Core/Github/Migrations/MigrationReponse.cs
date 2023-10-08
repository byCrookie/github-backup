using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github.Migrations;

internal sealed class MigrationReponse
{
    [JsonPropertyName("id")]
    public long Id { get; }
    
    [JsonPropertyName("state")]
    public MigrationState? State { get; }

    public MigrationReponse(long id, MigrationState? state)
    {
        Id = id;
        State = state;
    }
}