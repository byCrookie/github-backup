namespace GithubBackup.Cli.Utils;

internal static class ListExtensions
{
    public static ICollection<T> AddAll<T>(this ICollection<T> source, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            source.Add(item);
        }

        return source;
    }
    
    public static async IAsyncEnumerable<TResult> SelectAsync<TItem, TResult>(this IEnumerable<TItem> source, Func<TItem, Task<TResult>> selector)
    {
        foreach (var item in source)
        {
            yield return await selector(item);
        }
    }
}