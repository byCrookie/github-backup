namespace GithubBackup.Cli.Commands.Interval;

internal sealed class IntervalArgs
{
    public TimeSpan? Interval { get; }

    public IntervalArgs(TimeSpan? interval)
    {
        Interval = interval;
    }
}
