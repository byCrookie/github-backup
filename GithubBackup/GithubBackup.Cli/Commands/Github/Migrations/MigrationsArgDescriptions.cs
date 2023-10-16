namespace GithubBackup.Cli.Commands.Github.Migrations;

internal static class MigrationsArgDescriptions
{
    public static readonly Description Ids = new("Ids", "Id",
        """
        Only include the migration IDs in the output.
        Useful for piping into other commands.
        """
    );
}