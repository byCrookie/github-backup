using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace GithubBackup.TestUtils.Configuration;

public class KeyValueConfigurationProvider(IDictionary<string, string?> data)
    : IConfigurationProvider,
        IConfigurationSource
{
    public bool TryGet(string key, out string? value)
    {
        return data.TryGetValue(key, out value);
    }

    public void Set(string key, string? value)
    {
        data[key] = value;
    }

    public IChangeToken GetReloadToken()
    {
        return new CancellationChangeToken(CancellationToken.None);
    }

    public void Load() { }

    public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        return ArraySegment<string>.Empty;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return this;
    }
}
