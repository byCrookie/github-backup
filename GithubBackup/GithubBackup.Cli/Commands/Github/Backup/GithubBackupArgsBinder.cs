using System.CommandLine.Binding;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Backup;

public class GithubBackupArgsBinder : BinderBase<GithubBackupArgs>
{
    protected override GithubBackupArgs GetBoundValue(BindingContext bindingContext)
    {
        var destination = bindingContext.ParseResult.GetRequiredValueForOption(GithubBackupArgs.DestinationOption);
        return new GithubBackupArgs(destination);
    }
}