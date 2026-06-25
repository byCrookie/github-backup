using Spectre.Console;

namespace GithubBackup.Cli.Output;

internal static class DownloadProgress
{
    public static Task<T> RunAsync<T>(
        IAnsiConsole ansiConsole,
        string description,
        Func<Action<long, long?>, Task<T>> downloadAsync
    )
    {
        var progress = ansiConsole.Progress();
        progress.RefreshRate = TimeSpan.FromSeconds(5);

        return progress.StartAsync(ctx =>
        {
            var task = ctx.AddTask(description, maxValue: 100);
            return downloadAsync(
                (downloadedBytes, totalBytes) => Update(task, downloadedBytes, totalBytes)
            );
        });
    }

    private static void Update(ProgressTask task, long downloadedBytes, long? totalBytes)
    {
        if (totalBytes is null or 0)
        {
            task.IsIndeterminate = true;
            task.Description = $"Downloaded {FormatBytes(downloadedBytes)}";
            return;
        }

        task.IsIndeterminate = false;
        task.MaxValue = totalBytes.Value;
        task.Value = downloadedBytes;
        task.Description =
            $"Downloaded {FormatBytes(downloadedBytes)} of {FormatBytes(totalBytes.Value)}";
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var size = (double)bytes;
        var unit = 0;

        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.##} {units[unit]}";
    }
}
