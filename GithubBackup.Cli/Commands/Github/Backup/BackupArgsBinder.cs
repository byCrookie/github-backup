using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Interval;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal sealed class BackupArgsBinder(
    MigrateArguments migrateArguments,
    DownloadArguments downloadArguments,
    IntervalArguments intervalArguments,
    LoginArguments loginArguments)
{
    public BackupArgs Get(ParseResult parseResult)
    {
        var migrateArgs = new MigrateArgsBinder(
            migrateArguments,
            intervalArguments,
            loginArguments
        ).Get(parseResult);
        var downloadArgs = new DowndloadArgsBinder(
            downloadArguments,
            intervalArguments,
            loginArguments
        ).Get(parseResult);
        var intervalArgs = new IntervalArgsBinder(intervalArguments).Get(parseResult);
        var loginArgs = new LoginArgsBinder(loginArguments).Get(parseResult);
        return new BackupArgs(migrateArgs, downloadArgs, intervalArgs, loginArgs);
    }
}
