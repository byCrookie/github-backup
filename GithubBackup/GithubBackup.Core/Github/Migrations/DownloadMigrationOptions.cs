namespace GithubBackup.Core.Github.Migrations;

public class DownloadMigrationOptions
{
    public long Id { get; }
    public DirectoryInfo Destination { get; }
    public int? NumberOfBackups { get; }
    public bool Overwrite { get; }

    public DownloadMigrationOptions(long id, DirectoryInfo destination, int? numberOfBackups = null, bool overwrite = true)
    {
        Id = id;
        Destination = destination;
        NumberOfBackups = numberOfBackups;
        Overwrite = overwrite;
    }
}