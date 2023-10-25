using System.CommandLine.Binding;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal sealed class RepositoriesArgsBinder : BinderBase<RepositoriesArgs>
{
    private readonly RepositoriesArguments _repositoriesArguments;

    public RepositoriesArgsBinder(RepositoriesArguments repositoriesArguments)
    {
        _repositoriesArguments = repositoriesArguments;
    }
    
    protected override RepositoriesArgs GetBoundValue(BindingContext bindingContext)
    {
        var type = bindingContext.ParseResult.GetValueForOption(_repositoriesArguments.TypeOption);
        var affiliation = bindingContext.ParseResult.GetRequiredValueForOption(_repositoriesArguments.AffiliationOption);
        var visibility = bindingContext.ParseResult.GetRequiredValueForOption(_repositoriesArguments.VisibilityOption);
        return new RepositoriesArgs(type, affiliation, visibility);
    }
}