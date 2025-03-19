using System.CommandLine.Binding;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Interval;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal sealed class BackupArgsBinder : BinderBase<BackupArgs>
{
    private readonly MigrateArguments _migrateArguments;
    private readonly DownloadArguments _downloadArguments;
    private readonly IntervalArguments _intervalArguments;
    private readonly LoginArguments _loginArguments;

    public BackupArgsBinder(
        MigrateArguments migrateArguments,
        DownloadArguments downloadArguments,
        IntervalArguments intervalArguments,
        LoginArguments loginArguments
    )
    {
        _migrateArguments = migrateArguments;
        _downloadArguments = downloadArguments;
        _intervalArguments = intervalArguments;
        _loginArguments = loginArguments;
    }

    protected override BackupArgs GetBoundValue(BindingContext bindingContext)
    {
        var migrateArgs = new MigrateArgsBinder(
            _migrateArguments,
            _intervalArguments,
            _loginArguments
        ).Get(bindingContext);
        var downloadArgs = new DowndloadArgsBinder(
            _downloadArguments,
            _intervalArguments,
            _loginArguments
        ).Get(bindingContext);
        var intervalArgs = new IntervalArgsBinder(_intervalArguments).Get(bindingContext);
        var loginArgs = new LoginArgsBinder(_loginArguments).Get(bindingContext);
        return new BackupArgs(migrateArgs, downloadArgs, intervalArgs, loginArgs);
    }
}
