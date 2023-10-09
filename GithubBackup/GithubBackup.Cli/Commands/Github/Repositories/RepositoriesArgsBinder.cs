using System.CommandLine.Binding;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal sealed class RepositoriesArgsBinder : BinderBase<RepositoriesArgs>
{
    protected override RepositoriesArgs GetBoundValue(BindingContext bindingContext)
    {
        var type = bindingContext.ParseResult.GetRequiredValueForOption(RepositoriesArgs.TypeOption);
        var affiliation = bindingContext.ParseResult.GetRequiredValueForOption(RepositoriesArgs.AffiliationOption);
        var visibility = bindingContext.ParseResult.GetRequiredValueForOption(RepositoriesArgs.VisibilityOption);
        return new RepositoriesArgs(type, affiliation, visibility);
    }
}