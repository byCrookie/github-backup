using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github.Migrations;

internal sealed record MigrationReponse(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("state")] MigrationState? State,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt
);
