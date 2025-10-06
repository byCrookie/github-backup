using System.CommandLine;

namespace GithubBackup.Cli.Commands.Github.Migrations;

public class MigrationsArguments
{
    public Option<bool> ExportOption { get; } =
        new(
            name: "--export",
            aliases: ["-e"]
        )
        {
            Required = false,
            Description = MigrationsArgDescriptions.Export.Long,
            DefaultValueFactory = _ => true
        };

    public Option<DateTime?> SinceOption { get; } =
        new(
            name: "--since",
            aliases: ["-s"]
        )
        {
            Required = false,
            Description = MigrationsArgDescriptions.Since.Long
        };

    public Option<long?> DaysOldOption { get; } =
        new(
            name: "--days-old",
            aliases: ["-d"]
        )
        {
            Required = false,
            Description = MigrationsArgDescriptions.DaysOld.Long
        };

    public MigrationsArguments()
    {
        SinceOption.Validators.Add(result =>
        {
            var since = result.GetValue(SinceOption);
            var daysOld = result.GetValue(DaysOldOption);

            if (since is not null && daysOld is not null)
            {
                result.AddError(MigrationsArgsValidator.CannotSpecifySinceAndDaysOld);
            }
        });
    }

    public IEnumerable<Option> Options()
    {
        return [ExportOption, SinceOption, DaysOldOption];
    }
}
