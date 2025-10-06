using System.CommandLine;

namespace GithubBackup.Cli.Commands.Interval;

public class IntervalArguments
{
    public Option<long?> IntervalOption { get; } =
        new(
            name: "--interval",
            aliases: ["-i"]
        )
        {
            Required = false,
            Description = IntervalArgDescriptions.Interval.Long
        };

    public IEnumerable<Option> Options()
    {
        return new Option[] { IntervalOption };
    }
}
