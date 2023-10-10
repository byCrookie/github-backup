using System.Text.Json.Serialization;

namespace GithubBackup.Core.Tests.Flurl;

public class Item
{
    [JsonPropertyName("id")]
    public string Id { get; }

    public Item(string id)
    {
        Id = id;
    }
}