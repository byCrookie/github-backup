namespace GithubBackup.Core.Github.Authentication;

internal sealed class IntervalWrapper(TimeSpan interval)
{
    public TimeSpan Interval { get; private set; } = interval;

    public void Update(TimeSpan interval)
    {
        Interval = interval;
    }
}
