using System.CommandLine;

namespace GithubBackup.Cli.Commands.Interval;

internal sealed class IntervalArgsBinder
{
    private readonly IntervalArguments _intervalArguments;

    public IntervalArgsBinder(IntervalArguments intervalArguments)
    {
        _intervalArguments = intervalArguments;
    }

    public IntervalArgs Get(ParseResult parseResult)
    {
        var interval = parseResult.GetValue(
            _intervalArguments.IntervalOption
        );
        return new IntervalArgs(interval is null ? null : TimeSpan.FromSeconds(interval.Value));
    }
}
