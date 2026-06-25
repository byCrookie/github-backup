using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Download;

internal static class DownloadArgDescriptions
{
    public static readonly Description Migrations = new(
        "Migrations",
        "Migrations",
        $"""
        IDs of migrations to download.
        You can also pipe migration IDs using these separators: {Piping.Separators}.
        If not specified, the latest migration will be downloaded.
        """
    );

    public static readonly Description Latest = new(
        "Latest",
        "Latest",
        """
        Download only the latest migration.
        If the migration is not complete, it will be skipped unless --poll is specified.
        Ignored when migration IDs are provided.
        """
    );

    public static readonly Description Poll = new(
        "Poll",
        "Poll",
        "Wait until the selected migration is ready, then download it."
    );

    public static readonly Description Destination = new(
        "Destination",
        "Destination",
        "Directory where migration archives will be saved."
    );

    public static readonly Description NumberOfBackups = new(
        "NumberOfBackups",
        "NumberOfBackups",
        """
        Number of backup archives to keep.
        If not specified, all backups are kept.
        """
    );

    public static readonly Description Overwrite = new(
        "Overwrite",
        "Overwrite",
        "Overwrite existing backups."
    );
}
