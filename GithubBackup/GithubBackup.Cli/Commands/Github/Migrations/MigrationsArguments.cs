using System.CommandLine;

namespace GithubBackup.Cli.Commands.Github.Migrations;

public class MigrationsArguments
{
    public Option<bool> ExportOption { get; } = new(
        aliases: new[] { "-e", "--export" },
        getDefaultValue: () => true,
        description: MigrationsArgDescriptions.Export.Long
    ) { IsRequired = false };
    
    public Option<DateTime?> SinceOption { get; } = new(
        aliases: new[] { "-s", "--since" },
        description: MigrationsArgDescriptions.Since.Long
    ) { IsRequired = false };
    
    public Option<long?> DaysOldOption { get; } = new(
        aliases: new[] { "-d", "--days-old" },
        description: MigrationsArgDescriptions.DaysOld.Long
    ) { IsRequired = false };

    public MigrationsArguments()
    {
        SinceOption.AddValidator(result =>
        {
            var since = result.GetValueForOption(SinceOption);
            var daysOld = result.GetValueForOption(DaysOldOption);

            if (since is not null && daysOld is not null)
            {
                result.ErrorMessage = MigrationsArgsValidator.CannotSpecifySinceAndDaysOld;
            }
        });
    }

    public IEnumerable<Option> Options()
    {
        return new Option[]
        {
            ExportOption,
            SinceOption,
            DaysOldOption
        };
    }
}