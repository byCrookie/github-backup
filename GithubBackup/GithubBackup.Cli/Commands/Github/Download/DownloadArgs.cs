using GithubBackup.Cli.Commands.Github.Interval;
using GithubBackup.Cli.Commands.Services;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class DownloadArgs : ICommandIntervalArgs
{
    public long[] Migrations { get; }
    public bool Latest { get; }
    public DirectoryInfo Destination { get; }
    public int? NumberOfBackups { get; }
    public bool Overwrite { get; }
    public IntervalArgs IntervalArgs { get; }

    public DownloadArgs(
        long[] migrations,
        bool latest,
        DirectoryInfo destination,
        int? numberOfBackups,
        bool overwrite,
        IntervalArgs intervalArgs)
    {
        Migrations = migrations;
        Latest = latest;
        Destination = destination;
        NumberOfBackups = numberOfBackups;
        Overwrite = overwrite;
        IntervalArgs = intervalArgs;
    }
}