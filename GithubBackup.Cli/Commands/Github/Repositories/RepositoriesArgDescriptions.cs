namespace GithubBackup.Cli.Commands.Github.Repositories;

internal static class RepositoriesArgDescriptions
{
    public static readonly Description Type = new(
        "Type",
        "Type",
        """
        Only return repositories of the specified type.
        Can not be used with visibility or affiliation.
        """
    );

    public static readonly Description Affiliation = new(
        "Affiliation",
        "Affiliation",
        """
        Only return repositories of the specified affiliation.
        Will be ignored if type is specified.
        """
    );

    public static readonly Description Visibility = new(
        "Visibility",
        "Visibility",
        """
        Only return repositories of the specified visibility.
        Will be ignored if type is specified.
        """
    );
}
