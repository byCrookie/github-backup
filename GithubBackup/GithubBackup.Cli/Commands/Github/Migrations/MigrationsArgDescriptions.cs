namespace GithubBackup.Cli.Commands.Github.Migrations;

internal static class MigrationsArgDescriptions
{
    public static readonly Description Export = new("Long", "Long",
        "Only include valid migrations that can be exported."
    );
    
    public static readonly Description Since = new("Since", "Since",
        "Only include migrations that were created after the given date."
    );
    
    public static readonly Description DaysOld = new("DaysOld", "DaysOld",
        "Only include migrations that were create in the last given number of days."
    );
}