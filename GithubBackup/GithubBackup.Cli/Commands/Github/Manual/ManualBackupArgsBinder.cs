using System.CommandLine.Binding;

namespace GithubBackup.Cli.Commands.Github.Manual;

public class ManualBackupArgsBinder : BinderBase<ManualBackupArgs>
{
    protected override ManualBackupArgs GetBoundValue(BindingContext bindingContext)
    {
        return new ManualBackupArgs();
    }
}