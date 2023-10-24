namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class DownloadArgs
{
    public long[] Migrations { get; }
    public bool Latest { get; }
    public DirectoryInfo Destination { get; }
    public int? NumberOfBackups { get; }
    public bool Overwrite { get; }

    public DownloadArgs(
        long[] migrations,
        bool latest,
        DirectoryInfo destination,
        int? numberOfBackups,
        bool overwrite)
    {
        Migrations = migrations;
        Latest = latest;
        Destination = destination;
        NumberOfBackups = numberOfBackups;
        Overwrite = overwrite;
    }
}