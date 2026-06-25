namespace GithubBackup.Core.Github.Migrations;

public sealed class StartMigrationOptions(
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
    public string[] Repositories { get; } = repositories;
    public bool LockRepositories { get; } = lockRepositories;
    public bool ExcludeMetadata { get; } = excludeMetadata;
    public bool ExcludeGitData { get; } = excludeGitData;
    public bool ExcludeAttachements { get; } = excludeAttachements;
    public bool ExcludeReleases { get; } = excludeReleases;
    public bool ExcludeOwnerProjects { get; } = excludeOwnerProjects;
    public bool ExcludeMetadataOnly { get; } = excludeMetadataOnly;
}
