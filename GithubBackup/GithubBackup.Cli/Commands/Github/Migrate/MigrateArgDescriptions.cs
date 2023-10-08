namespace GithubBackup.Cli.Commands.Github.Migrate;

public static class MigrateArgDescriptions
{
    public static Description Repositories = new("Repositories", "Repositories", "The repositories to backup e.g. 'owner/repo'");
    public static Description LockRepositories = new("LockRepositories", "Lock Repositories", "Lock the repositories being migrated at the start of the migration");
    public static Description ExcludeMetadata = new("ExcludeMetadata", "Exclude Metadata", "Indicates whether metadata should be excluded and only git source should be included for the migration.");
    public static Description ExcludeGitData = new("ExcludeGitData", "Exclude Git Data", "Indicates whether the repository git data should be excluded from the migration.");
    public static Description ExcludeAttachements = new("ExcludeAttachements", "Exclude Attachements", "Do not include attachments in the migration");
    public static Description ExcludeReleases = new("ExcludeReleases", "Exclude Releases", "Do not include releases in the migration");
    public static Description ExcludeOwnerProjects = new("ExcludeOwnerProjects", "Exclude Owner Projects", "Indicates whether projects owned by the organization or users should be excluded.");
    public static Description ExcludeMetadataOnly = new("ExcludeMetadataOnly", "Exclude Metadata Only", "Indicates whether this should only include organization metadata (repositories array should be empty and will ignore other flags).");
    
    public static readonly Description[] All = {
        Repositories,
        LockRepositories,
        ExcludeMetadata,
        ExcludeGitData,
        ExcludeAttachements,
        ExcludeReleases,
        ExcludeOwnerProjects,
        ExcludeMetadataOnly
    };
    
    public static Description GetDescription(string name)
    {
        return All.First(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}