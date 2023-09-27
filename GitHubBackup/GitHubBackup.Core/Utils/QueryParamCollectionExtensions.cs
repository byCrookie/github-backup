using Flurl;

namespace GitHubBackup.Core.Utils;

public static class QueryParamCollectionExtensions
{
    public static T Get<T>(this QueryParamCollection query, string key)
    {
        if (query.TryGetFirst(key, out var value))
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        
        throw new InvalidOperationException($"Query parameter '{key}' not found.");
    }
}