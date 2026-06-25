using Flurl.Util;

namespace GithubBackup.Core.Utils;

internal static class CollectionExtensions
{
    extension<T>(INameValueListBase<T> query)
        where T : class
    {
        public T GetRequired(string key)
        {
            return query.TryGetFirst(key, out var value)
                ? value
                : throw new InvalidOperationException($"Query parameter '{key}' not found.");
        }

        public T? Get(string key)
        {
            return query.TryGetFirst(key, out var value) ? value : null;
        }
    }
}
