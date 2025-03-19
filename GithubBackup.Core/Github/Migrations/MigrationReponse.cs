using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github.Migrations;

internal sealed class MigrationReponse
{
    [JsonPropertyName("id")]
    public long Id { get; }

    [JsonPropertyName("state")]
    public MigrationState? State { get; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; }

    public MigrationReponse(long id, MigrationState? state, DateTime createdAt)
    {
        Id = id;
        State = state;
        CreatedAt = createdAt;
    }
}
