namespace GithubBackup.Cli.Commands.Github.Migrations;

internal static class MigrationsArgDescriptions
{
    public static readonly Description Long = new("Long", "Long",
        """
        Full details of the migrations are included in the output.
        If not supplied, only the migration IDs are included in the output.
        """
    );
}