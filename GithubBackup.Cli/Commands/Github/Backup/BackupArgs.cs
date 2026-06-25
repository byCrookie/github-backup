using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Commands.Services;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal sealed record BackupArgs(
    MigrateArgs MigrateArgs,
    DownloadArgs DownloadArgs,
    IntervalArgs IntervalArgs,
    LoginArgs LoginArgs
) : ICommandIntervalArgs;
