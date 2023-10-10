using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Migrate;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal sealed class BackupArgs
{
    public MigrateArgs MigrateArgs { get; }
    public DownloadArgs DownloadArgs { get; }

    public BackupArgs(MigrateArgs migrateArgs, DownloadArgs downloadArgs)
    {
        MigrateArgs = migrateArgs;
        DownloadArgs = downloadArgs;
    }
}