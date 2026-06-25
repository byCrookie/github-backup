using System.IO.Abstractions;

namespace GithubBackup.Core.Github.Migrations;

public sealed class DownloadMigrationOptions(
    long id,
    IDirectoryInfo destination,
    int? numberOfBackups = null,
    bool overwrite = true,
    TimeSpan? medianFirstRetryDelay = null,
    Action<string>? onTemporaryFileCreated = null,
    Action<long, long?>? onDownloadProgress = null
)
{
    public long Id { get; } = id;
    public IDirectoryInfo Destination { get; } = destination;
    public int? NumberOfBackups { get; } = numberOfBackups;
    public bool Overwrite { get; } = overwrite;
    public TimeSpan MedianFirstRetryDelay { get; } =
        medianFirstRetryDelay ?? TimeSpan.FromMinutes(1);
    public Action<string>? OnTemporaryFileCreated { get; } = onTemporaryFileCreated;
    public Action<long, long?>? OnDownloadProgress { get; } = onDownloadProgress;
}
