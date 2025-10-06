using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Login;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal sealed class RepositoriesArgsBinder
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

    public RepositoriesArgs Get(ParseResult parseResult)
    {
        var type = parseResult.GetValue(_repositoriesArguments.TypeOption);
        var affiliation = parseResult.GetRequiredValue(
            _repositoriesArguments.AffiliationOption
        );
        var visibility = parseResult.GetRequiredValue(
            _repositoriesArguments.VisibilityOption
        );

        var loginArgs = new LoginArgsBinder(_loginArguments).Get(parseResult);

        return new RepositoriesArgs(type, affiliation, visibility, loginArgs);
    }
}
