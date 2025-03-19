using Microsoft.Extensions.Caching.Memory;

namespace GithubBackup.Core.Tests.Utils;

public class NullCache : IMemoryCache
{
    public void Dispose() { }

    public bool TryGetValue(object key, out object? value)
    {
        value = null;
        return false;
    }

    public ICacheEntry CreateEntry(object key)
    {
        throw new NotSupportedException();
    }

    public void Remove(object key) { }
}
