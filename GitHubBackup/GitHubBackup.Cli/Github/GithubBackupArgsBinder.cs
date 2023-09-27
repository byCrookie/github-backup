using System.CommandLine.Binding;
using GitHubBackup.Cli.Utils;

namespace GitHubBackup.Cli.Github;

public class GithubBackupArgsBinder : BinderBase<GithubBackupArgs>
{
    protected override GithubBackupArgs GetBoundValue(BindingContext bindingContext)
    {
        var destination = bindingContext.ParseResult.GetRequiredValueForOption(GithubBackupArgs.DestinationOption);
        return new GithubBackupArgs(destination);
    }
}