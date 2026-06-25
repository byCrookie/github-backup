using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github.Migrations;

internal sealed class MigrationRequest(
    string[] repositories,
    bool lockRepositories,
    bool excludeMetadata,
    bool excludeGitData,
    bool excludeAttachements,
    bool excludeReleases,
    bool excludeOwnerProjects,
    bool orgMetadataOnly
)
{
    [JsonPropertyName("repositories")]
    public string[] Repositories { get; } = repositories;

    [JsonPropertyName("exclude")]
    public string[] Exclude { get; } = ["repositories"];

    [JsonPropertyName("lock_repositories")]
    public bool LockRepositories { get; } = lockRepositories;

    [JsonPropertyName("exclude_metadata")]
    public bool ExcludeMetadata { get; } = excludeMetadata;

    [JsonPropertyName("exclude_git_data")]
    public bool ExcludeGitData { get; } = excludeGitData;

    [JsonPropertyName("exclude_attachments")]
    public bool ExcludeAttachements { get; } = excludeAttachements;

    [JsonPropertyName("exclude_releases")]
    public bool ExcludeReleases { get; } = excludeReleases;

    [JsonPropertyName("exclude_owner_projects")]
    public bool ExcludeOwnerProjects { get; } = excludeOwnerProjects;

    [JsonPropertyName("org_metadata_only")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool OrgMetadataOnly { get; } = orgMetadataOnly;
}
