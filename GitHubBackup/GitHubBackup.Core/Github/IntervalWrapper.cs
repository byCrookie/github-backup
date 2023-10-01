namespace GithubBackup.Core.Github;

public class IntervalWrapper
{
    public TimeSpan Interval { get; private set; }

    public IntervalWrapper(TimeSpan interval)
    {
        Interval = interval;
    }
    
    public void Update(TimeSpan interval)
    {
        Interval = interval;
    }
}