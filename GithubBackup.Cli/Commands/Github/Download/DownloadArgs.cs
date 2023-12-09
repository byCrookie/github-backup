using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Commands.Services;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class DownloadArgs : ICommandIntervalArgs
{
    public long[] Migrations { get; }
    public bool Latest { get; }
    public bool Poll { get; }
    public DirectoryInfo Destination { get; }
    public int? NumberOfBackups { get; }
    public bool Overwrite { get; }
    public IntervalArgs IntervalArgs { get; }
    public LoginArgs LoginArgs { get; }

    public DownloadArgs(
        long[] migrations,
        bool latest,
        bool poll,
        DirectoryInfo destination,
        int? numberOfBackups,
        bool overwrite,
        IntervalArgs intervalArgs,
        LoginArgs loginArgs)
    {
        Migrations = migrations;
        Latest = latest;
        Poll = poll;
        Destination = destination;
        NumberOfBackups = numberOfBackups;
        Overwrite = overwrite;
        IntervalArgs = intervalArgs;
        LoginArgs = loginArgs;
    }
}