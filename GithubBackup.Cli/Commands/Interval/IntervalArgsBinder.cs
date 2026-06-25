using System.CommandLine;

namespace GithubBackup.Cli.Commands.Interval;

internal sealed class IntervalArgsBinder(IntervalArguments intervalArguments)
{
    public IntervalArgs Get(ParseResult parseResult)
    {
        var interval = parseResult.GetValue(intervalArguments.IntervalOption);
        return new IntervalArgs(interval is null ? null : TimeSpan.FromSeconds(interval.Value));
    }
}
