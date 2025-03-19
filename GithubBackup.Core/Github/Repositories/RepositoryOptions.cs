namespace GithubBackup.Core.Github.Repositories;

public class RepositoryOptions
{
    public RepositoryType? Type { get; }
    public RepositoryAffiliation? Affiliation { get; }
    public RepositoryVisibility? Visibility { get; }

    public RepositoryOptions(
        RepositoryType? type = null,
        RepositoryAffiliation? affiliation = RepositoryAffiliation.Owner,
        RepositoryVisibility? visibility = RepositoryVisibility.All
    )
    {
        Type = type;
        Affiliation = affiliation;
        Visibility = visibility;
    }
}
