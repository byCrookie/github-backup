using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal static class MigrateArgDescriptions
{
    public static readonly Description Repositories = new(
        "Repositories",
        "Repositories",
        $"""
        Repositories to back up, for example 'owner/repo'.
        You can also pipe repository names using these separators: {Piping.Separators}.
        """
    );

    public static readonly Description LockRepositories = new(
        "LockRepositories",
        "Lock Repositories",
        "Lock repositories while GitHub exports the migration."
    );

    public static readonly Description ExcludeMetadata = new(
        "ExcludeMetadata",
        "Exclude Metadata",
        "Exclude repository metadata and include only Git source data."
    );

    public static readonly Description ExcludeGitData = new(
        "ExcludeGitData",
        "Exclude Git Data",
        "Exclude repository Git data from the migration."
    );

    public static readonly Description ExcludeAttachements = new(
        "ExcludeAttachments",
        "Exclude Attachments",
        "Do not include attachments in the migration."
    );

    public static readonly Description ExcludeReleases = new(
        "ExcludeReleases",
        "Exclude Releases",
        "Do not include releases in the migration."
    );

    public static readonly Description ExcludeOwnerProjects = new(
        "ExcludeOwnerProjects",
        "Exclude Owner Projects",
        "Exclude projects owned by the organization or user."
    );

    public static readonly Description OrgMetadataOnly = new(
        "OrgMetadataOnly",
        "Org Metadata Only",
        "Export organization metadata only. Do not specify repositories with this option."
    );
}
