using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Login;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal sealed class RepositoriesArgsBinder(
    RepositoriesArguments repositoriesArguments,
    LoginArguments loginArguments
)
{
    public RepositoriesArgs Get(ParseResult parseResult)
    {
        var type = parseResult.GetValue(repositoriesArguments.TypeOption);
        var affiliation = parseResult.GetRequiredValue(repositoriesArguments.AffiliationOption);
        var visibility = parseResult.GetRequiredValue(repositoriesArguments.VisibilityOption);

        var loginArgs = new LoginArgsBinder(loginArguments).Get(parseResult);

        return new RepositoriesArgs(type, affiliation, visibility, loginArgs);
    }
}
