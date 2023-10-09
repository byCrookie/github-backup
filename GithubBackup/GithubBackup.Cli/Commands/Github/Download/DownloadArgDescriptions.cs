namespace GithubBackup.Cli.Commands.Github.Download;

internal static class DownloadArgDescriptions
{
    public static readonly Description Migrations = new("Migrations", "Migrations", "Id's of the migrations to download. If not specified, all migrations will be downloaded. If a migration is not yet completed, it will be skipped.");
    public static readonly Description Latest = new("Latest", "Latest", "Only download the latest migration. If the migration is not yet completed, it will be skipped.");
    public static readonly Description Destination = new("Destination", "Destination", "The destination directory to download the migrations to.");
    public static readonly Description NumberOfBackups = new("NumberOfBackups", "NumberOfBackups", "The number of backups to keep. If not specified, all backups will be kept.");
    public static readonly Description Overwrite = new("Overwrite", "Overwrite", "Overwrite existing backups.");
}