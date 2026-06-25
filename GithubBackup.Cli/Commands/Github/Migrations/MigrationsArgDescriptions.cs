namespace GithubBackup.Cli.Commands.Github.Migrations;

internal static class MigrationsArgDescriptions
{
    public static readonly Description Export = new(
        "Long",
        "Long",
        "Show only migrations that can still be downloaded."
    );

    public static readonly Description Since = new(
        "Since",
        "Since",
        "Show only migrations created on or after this date."
    );

    public static readonly Description DaysOld = new(
        "DaysOld",
        "DaysOld",
        "Show only migrations created within the last N days."
    );
}
