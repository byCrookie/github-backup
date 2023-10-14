using System.Diagnostics;

namespace GithubBackup.Core.Tests.Utils;

public static class AssertionExtensions
{
    public static async Task<TimeSpan> ExecutionTimeAsync(this Func<Task> action)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        await action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }
}