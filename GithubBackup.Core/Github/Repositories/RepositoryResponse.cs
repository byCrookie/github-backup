using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github.Repositories;

internal sealed record RepositoryResponse(
    [property: JsonPropertyName("full_name")] string FullName
);
