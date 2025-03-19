using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Commands.Services;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal sealed class BackupArgs : ICommandIntervalArgs
{
    public MigrateArgs MigrateArgs { get; }
    public DownloadArgs DownloadArgs { get; }
    public IntervalArgs IntervalArgs { get; }
    public LoginArgs LoginArgs { get; }

    public BackupArgs(
        MigrateArgs migrateArgs,
        DownloadArgs downloadArgs,
        IntervalArgs intervalArgs,
        LoginArgs loginArgs
    )
    {
        MigrateArgs = migrateArgs;
        DownloadArgs = downloadArgs;
        IntervalArgs = intervalArgs;
        LoginArgs = loginArgs;
    }
}
