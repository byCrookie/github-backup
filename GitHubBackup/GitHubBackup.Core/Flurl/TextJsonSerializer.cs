using System.Text.Json;
using Flurl.Http.Configuration;

namespace GithubBackup.Core.Flurl;

public class TextJsonSerializer : ISerializer
{
    public string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj);
    }

    public T? Deserialize<T>(string s)
    {
        return JsonSerializer.Deserialize<T>(s);
    }

    public T? Deserialize<T>(Stream stream)
    {
        return JsonSerializer.Deserialize<T>(stream);
    }
}