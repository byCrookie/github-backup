using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Core.Github.Repositories;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal sealed class RepositoriesArgs
{
    public RepositoryType? Type { get; }
    public RepositoryAffiliation? Affiliation { get; }
    public RepositoryVisibility? Visibility { get; }
    public LoginArgs LoginArgs { get; }

    public RepositoriesArgs(
        RepositoryType? type, 
        RepositoryAffiliation? affiliation,
        RepositoryVisibility? visibility,
        LoginArgs loginArgs)
    {
        Type = type;
        Affiliation = affiliation;
        Visibility = visibility;
        LoginArgs = loginArgs;
    }
}