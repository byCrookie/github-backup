namespace GithubBackup.Cli.Commands.Github.Repositories;

internal static class RepositoriesArgDescriptions
{
    public static readonly Description Type = new(
        "Type",
        "Type",
        """
        Return only repositories of this type.
        Cannot be used with visibility or affiliation filters.
        """
    );

    public static readonly Description Affiliation = new(
        "Affiliation",
        "Affiliation",
        """
        Return only repositories with this affiliation.
        Ignored when type is specified.
        """
    );

    public static readonly Description Visibility = new(
        "Visibility",
        "Visibility",
        """
        Return only repositories with this visibility.
        Ignored when type is specified.
        """
    );
}
