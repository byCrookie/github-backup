using System.CommandLine.Binding;

namespace GithubBackup.Cli.Commands.Github.Manual;

internal sealed class ManualBackupArgsBinder : BinderBase<ManualBackupArgs>
{
    protected override ManualBackupArgs GetBoundValue(BindingContext bindingContext)
    {
        return new ManualBackupArgs();
    }
}