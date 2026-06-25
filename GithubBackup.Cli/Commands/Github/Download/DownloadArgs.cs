using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Commands.Services;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed record DownloadArgs(
    long[] Migrations,
    bool Latest,
    bool Poll,
    DirectoryInfo Destination,
    int? NumberOfBackups,
    bool Overwrite,
    IntervalArgs IntervalArgs,
    LoginArgs LoginArgs
) : ICommandIntervalArgs;
