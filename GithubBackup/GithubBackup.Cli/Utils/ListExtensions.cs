namespace GithubBackup.Cli.Utils;

public static class ListExtensions
{
    public static ICollection<T> AddAll<T>(this ICollection<T> source, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            source.Add(item);
        }

        return source;
    }
}