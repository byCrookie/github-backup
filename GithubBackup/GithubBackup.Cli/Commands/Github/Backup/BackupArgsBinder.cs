using System.CommandLine.Binding;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Interval;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal sealed class BackupArgsBinder : BinderBase<BackupArgs>
{
    private readonly MigrateArguments _migrateArguments;
    private readonly DownloadArguments _downloadArguments;
    private readonly IntervalArguments _intervalArguments;

    public BackupArgsBinder(MigrateArguments migrateArguments, DownloadArguments downloadArguments, IntervalArguments intervalArguments)
    {
        _migrateArguments = migrateArguments;
        _downloadArguments = downloadArguments;
        _intervalArguments = intervalArguments;
    }

    protected override BackupArgs GetBoundValue(BindingContext bindingContext)
    {
        var migrateArgs = new MigrateArgsBinder(_migrateArguments, _intervalArguments).Get(bindingContext);
        var downloadArgs = new DowndloadArgsBinder(_downloadArguments, _intervalArguments).Get(bindingContext);
        var intervalArgs = new IntervalArgsBinder(_intervalArguments).Get(bindingContext);
        return new BackupArgs(migrateArgs, downloadArgs, intervalArgs);
    }
}