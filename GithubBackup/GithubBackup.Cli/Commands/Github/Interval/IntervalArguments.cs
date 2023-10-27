using System.CommandLine;

namespace GithubBackup.Cli.Commands.Github.Interval;

public class IntervalArguments
{
    public Option<long?> IntervalOption { get; } = new(
        aliases: new[] { "-i", "--interval" },
        description: IntervalArgDescriptions.Interval.Long
    ) { IsRequired = false };

    public IEnumerable<Option> Options()
    {
        return new Option[]
        {
            IntervalOption
        };
    }
}