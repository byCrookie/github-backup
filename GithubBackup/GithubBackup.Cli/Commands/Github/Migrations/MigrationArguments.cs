using System.CommandLine;
using GithubBackup.Cli.Commands.Global;

namespace GithubBackup.Cli.Commands.Github.Migrations;

public class MigrationArguments
{
    public Option<bool> IdOption { get; }

    public MigrationArguments()
    {
        IdOption = new Option<bool>(
            aliases: new[] { "-id", "--only-ids" },
            getDefaultValue: () => true,
            description: MigrationsArgDescriptions.Ids.Long
        ) { IsRequired = false };

        IdOption.AddValidator(result =>
        {
            var interactive = result.GetValueForOption(GlobalArgs.InteractiveOption);

            if (interactive && result.GetValueForOption(IdOption))
            {
                result.ErrorMessage = "The '-id / --only-ids' option cannot be used in interactive mode.";
            }
        });
    }
    
    public Option[] Options()
    {
        return new Option[]
        {
            IdOption
        };
    }
}