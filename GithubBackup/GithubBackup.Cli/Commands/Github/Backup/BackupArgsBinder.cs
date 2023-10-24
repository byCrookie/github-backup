using System.CommandLine.Binding;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Migrate;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal sealed class BackupArgsBinder : BinderBase<BackupArgs>
{
    private readonly MigrateArguments _migrateArguments;
    private readonly DownloadArguments _downloadArguments;

    public BackupArgsBinder(MigrateArguments migrateArguments, DownloadArguments downloadArguments)
    {
        _migrateArguments = migrateArguments;
        _downloadArguments = downloadArguments;
    }

    protected override BackupArgs GetBoundValue(BindingContext bindingContext)
    {
        var migrateArgs = new MigrateArgsBinder(_migrateArguments).Get(bindingContext);
        var downloadArgs = new DowndloadArgsBinder(_downloadArguments).Get(bindingContext);
        return new BackupArgs(migrateArgs, downloadArgs);
    }
}