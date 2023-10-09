using System.CommandLine;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class MigrationsArgs
{
    public bool Id { get; }

    public MigrationsArgs(bool id)
    {
        Id = id;
    }
    
    public static Option<bool> IdOption { get; }

    static MigrationsArgs()
    {
        IdOption = new Option<bool>(
            aliases: new[] { "-id", "--only-ids" },
            getDefaultValue: () => true,
            description: MigrationsArgDescriptions.Ids.Long
        ) { IsRequired = false };
    }
}