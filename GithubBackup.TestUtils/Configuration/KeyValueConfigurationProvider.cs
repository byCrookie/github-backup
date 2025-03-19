using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace GithubBackup.TestUtils.Configuration;

public class KeyValueConfigurationProvider : IConfigurationProvider, IConfigurationSource
{
    private readonly IDictionary<string, string?> _data;

    public KeyValueConfigurationProvider(IDictionary<string, string?> data)
    {
        _data = data;
    }

    public bool TryGet(string key, out string? value)
    {
        return _data.TryGetValue(key, out value);
    }

    public void Set(string key, string? value)
    {
        _data[key] = value;
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
