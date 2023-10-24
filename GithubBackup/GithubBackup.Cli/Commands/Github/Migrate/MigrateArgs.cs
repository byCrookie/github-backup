using FluentValidation;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal sealed class MigrateArgs
{
    public string[] Repositories { get; }
    public bool LockRepositories { get; }
    public bool ExcludeMetadata { get; }
    public bool ExcludeGitData { get; }
    public bool ExcludeAttachements { get; }
    public bool ExcludeReleases { get; }
    public bool ExcludeOwnerProjects { get; }
    public bool OrgMetadataOnly { get; }

    public MigrateArgs(
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
        Repositories = repositories;
        LockRepositories = lockRepositories;
        ExcludeMetadata = excludeMetadata;
        ExcludeGitData = excludeGitData;
        ExcludeAttachements = excludeAttachements;
        ExcludeReleases = excludeReleases;
        ExcludeOwnerProjects = excludeOwnerProjects;
        OrgMetadataOnly = orgMetadataOnly;
        
        new MigrateArgsValidator().ValidateAndThrow(this);
    }
}