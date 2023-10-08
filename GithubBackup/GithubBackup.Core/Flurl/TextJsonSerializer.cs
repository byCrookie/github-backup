using System.Text.Json;
using System.Text.Json.Serialization;
using Flurl.Http.Configuration;

namespace GithubBackup.Core.Flurl;

internal sealed class TextJsonSerializer : ISerializer
{
    private static JsonSerializerOptions Options => new()
    {
        Converters = { new JsonStringEnumConverter() }
    };
    
    public string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, Options);
    }

    public T? Deserialize<T>(string s)
    {
        return JsonSerializer.Deserialize<T>(s, Options);
    }

    public T? Deserialize<T>(Stream stream)
    {
        return JsonSerializer.Deserialize<T>(stream, Options);
    }
}