namespace GithubBackup.Core.Github.Repositories;

public class RepositoryOptions(
    RepositoryType? type = null,
    RepositoryAffiliation? affiliation = RepositoryAffiliation.Owner,
    RepositoryVisibility? visibility = RepositoryVisibility.All
)
{
    public RepositoryType? Type { get; } = type;
    public RepositoryAffiliation? Affiliation { get; } = affiliation;
    public RepositoryVisibility? Visibility { get; } = visibility;
}
