using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github.Migrations;

internal class MigrationReponse
{
    [JsonPropertyName("id")]
    public long Id { get; }

    public MigrationReponse(long id)
    {
        Id = id;
    }
}