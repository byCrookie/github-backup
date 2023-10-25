using System.CommandLine;

namespace GithubBackup.Cli.Commands.Github.Migrations;

public class MigrationsArguments
{
    public Option<bool> LongOption { get; } = new(
        aliases: new[] { "-l", "--long" },
        getDefaultValue: () => false,
        description: MigrationsArgDescriptions.Long.Long
    ) { IsRequired = false };

    public IEnumerable<Option> Options()
    {
        return new Option[]
        {
            LongOption
        };
    }
}