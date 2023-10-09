using System.CommandLine;
using GithubBackup.Cli.Options;

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

        IdOption.AddValidator(result =>
        {
            var interactive = result.GetValueForOption(GlobalArgs.InteractiveOption);

            if (interactive && result.GetValueForOption(IdOption))
            {
                result.ErrorMessage = "The '-id / --only-ids' option cannot be used in interactive mode.";
            }
        });
    }
}