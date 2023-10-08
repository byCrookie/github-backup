using System.CommandLine.Binding;

namespace GithubBackup.Cli.Commands.Github.Manual;

internal class ManualBackupArgsBinder : BinderBase<ManualBackupArgs>
{
    protected override ManualBackupArgs GetBoundValue(BindingContext bindingContext)
    {
        return new ManualBackupArgs();
    }
}