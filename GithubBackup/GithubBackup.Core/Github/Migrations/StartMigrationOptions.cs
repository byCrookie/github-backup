namespace GithubBackup.Core.Github.Migrations;

public sealed class StartMigrationOptions
{
    public string[] Repositories { get; }
    public bool LockRepositories { get; }
    public bool ExcludeMetadata { get; }
    public bool ExcludeGitData { get; }
    public bool ExcludeAttachements { get; }
    public bool ExcludeReleases { get; }
    public bool ExcludeOwnerProjects { get; }
    public bool ExcludeMetadataOnly { get; }

    public StartMigrationOptions(
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