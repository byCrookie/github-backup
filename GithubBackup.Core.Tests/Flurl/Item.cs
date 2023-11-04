using System.Text.Json.Serialization;

namespace GithubBackup.Core.Tests.Flurl;

internal class Item
{
    [JsonPropertyName("id")]
    public string Id { get; }

    [JsonPropertyName("itemEnum")]
    public ItemEnum ItemEnum { get; }

    public Item(string id, ItemEnum itemEnum)
    {
        Id = id;
        ItemEnum = itemEnum;
    }
}