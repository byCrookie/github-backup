namespace GithubBackup.Cli.Commands.Github.Migrate;

public static class MigrateArgDescriptions
{
    public static readonly Description Repositories = new("Repositories", "Repositories", "The repositories to backup e.g. 'owner/repo'");
    public static readonly Description LockRepositories = new("LockRepositories", "Lock Repositories", "Lock the repositories being migrated at the start of the migration");
    public static readonly Description ExcludeMetadata = new("ExcludeMetadata", "Exclude Metadata", "Indicates whether metadata should be excluded and only git source should be included for the migration.");
    public static readonly Description ExcludeGitData = new("ExcludeGitData", "Exclude Git Data", "Indicates whether the repository git data should be excluded from the migration.");
    public static readonly Description ExcludeAttachements = new("ExcludeAttachements", "Exclude Attachements", "Do not include attachments in the migration");
    public static readonly Description ExcludeReleases = new("ExcludeReleases", "Exclude Releases", "Do not include releases in the migration");
    public static readonly Description ExcludeOwnerProjects = new("ExcludeOwnerProjects", "Exclude Owner Projects", "Indicates whether projects owned by the organization or users should be excluded.");
    public static readonly Description ExcludeMetadataOnly = new("ExcludeMetadataOnly", "Exclude Metadata Only", "Indicates whether this should only include organization metadata (repositories array should be empty and will ignore other flags).");
}