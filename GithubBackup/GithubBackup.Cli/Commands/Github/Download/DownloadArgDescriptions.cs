using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Download;

internal static class DownloadArgDescriptions
{
    public static readonly Description Migrations = new("Migrations", "Migrations",
        $"""
         Id's of the migrations to download.
         Instead piping can be used to provide the id's. Supported separators: {StandardInput.Separators.Values}.
         If not specified, the latest migration will be downloaded.
         """
    );

    public static readonly Description Latest = new("Latest", "Latest",
        """
        Only download the latest migration.
        If the migration is not yet completed, it will be skipped.
        If id's are specified, this flag will be ignored.
        """
    );

    public static readonly Description Destination = new("Destination", "Destination",
        "The destination directory to download the migrations to.");

    public static readonly Description NumberOfBackups = new("NumberOfBackups", "NumberOfBackups",
        """
        The number of backups to keep.
        If not specified, all backups will be kept.
        """
    );

    public static readonly Description Overwrite = new("Overwrite", "Overwrite", "Overwrite existing backups.");
}