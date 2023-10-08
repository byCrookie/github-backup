using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github.Migrations;

internal class MigrationRequest
{
    [JsonPropertyName("repositories")]
    public string[] Repositories { get; }

    [JsonPropertyName("exclude")]
    public string[] Exclude { get; }

    [JsonPropertyName("lock_repositories")]
    public bool LockRepositories { get; }

    [JsonPropertyName("exclude_metadata")]
    public bool ExcludeMetadata { get; }

    [JsonPropertyName("exclude_git_data")]
    public bool ExcludeGitData { get; }

    [JsonPropertyName("exclude_attachments")]
    public bool ExcludeAttachements { get; }

    [JsonPropertyName("exclude_releases")]
    public bool ExcludeReleases { get; }

    [JsonPropertyName("exclude_owner_projects")]
    public bool ExcludeOwnerProjects { get; }

    [JsonPropertyName("exclude_metadata_only")]
    public bool ExcludeMetadataOnly { get; }

    public MigrationRequest(
        string[] repositories,
        bool lockRepositories,
        bool excludeMetadata,
        bool excludeGitData,
        bool excludeAttachements,
        bool excludeReleases,
        bool excludeOwnerProjects,
        bool excludeMetadataOnly
    )
    {
        Exclude = new[] { "repositories" };
        Repositories = repositories;
        LockRepositories = lockRepositories;
        ExcludeMetadata = excludeMetadata;
        ExcludeGitData = excludeGitData;
        ExcludeAttachements = excludeAttachements;
        ExcludeReleases = excludeReleases;
        ExcludeOwnerProjects = excludeOwnerProjects;
        ExcludeMetadataOnly = excludeMetadataOnly;
    }
}