using System.IO.Abstractions;

namespace GithubBackup.Core.Github.Migrations;

public sealed class DownloadMigrationOptions
{
    public long Id { get; }
    public IDirectoryInfo Destination { get; }
    public int? NumberOfBackups { get; }
    public bool Overwrite { get; }
    public TimeSpan MedianFirstRetryDelay { get; }

    public DownloadMigrationOptions(
        long id,
        IDirectoryInfo destination,
        int? numberOfBackups = null,
        bool overwrite = true,
        TimeSpan? medianFirstRetryDelay = null)
    {
        Id = id;
        Destination = destination;
        NumberOfBackups = numberOfBackups;
        Overwrite = overwrite;
        MedianFirstRetryDelay = medianFirstRetryDelay ?? TimeSpan.FromMinutes(1);
    }
}