using System.CommandLine.Binding;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal sealed class RepositoriesArgsBinder : BinderBase<RepositoriesArgs>
{
    private readonly RepositoriesArguments _repositoriesArguments;
    private readonly LoginArguments _loginArguments;

    public RepositoriesArgsBinder(
        RepositoriesArguments repositoriesArguments,
        LoginArguments loginArguments
    )
    {
        _repositoriesArguments = repositoriesArguments;
        _loginArguments = loginArguments;
    }

    protected override RepositoriesArgs GetBoundValue(BindingContext bindingContext)
    {
        var type = bindingContext.ParseResult.GetValueForOption(_repositoriesArguments.TypeOption);
        var affiliation = bindingContext.ParseResult.GetRequiredValueForOption(
            _repositoriesArguments.AffiliationOption
        );
        var visibility = bindingContext.ParseResult.GetRequiredValueForOption(
            _repositoriesArguments.VisibilityOption
        );

        var loginArgs = new LoginArgsBinder(_loginArguments).Get(bindingContext);

        return new RepositoriesArgs(type, affiliation, visibility, loginArgs);
    }
}
