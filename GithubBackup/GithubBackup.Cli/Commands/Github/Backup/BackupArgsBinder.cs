using System.CommandLine.Binding;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Migrate;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal sealed class BackupArgsBinder : BinderBase<BackupArgs>
{
    protected override BackupArgs GetBoundValue(BindingContext bindingContext)
    {
        var migrateArgs = new MigrateArgsBinder().Get(bindingContext);
        var downloadArgs = new DowndloadArgsBinder().Get(bindingContext);
        return new BackupArgs(migrateArgs, downloadArgs);
    }
}