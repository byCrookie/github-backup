using GithubBackup.Core.Github.Repositories;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal sealed class RepositoriesArgs
{
    public RepositoryType? Type { get; }
    public RepositoryAffiliation? Affiliation { get; }
    public RepositoryVisibility? Visibility { get; }

    public RepositoriesArgs(
        RepositoryType? type = null, 
        RepositoryAffiliation? affiliation = RepositoryAffiliation.Owner,
        RepositoryVisibility? visibility = RepositoryVisibility.All)
    {
        Type = type;
        Affiliation = affiliation;
        Visibility = visibility;
    }
}