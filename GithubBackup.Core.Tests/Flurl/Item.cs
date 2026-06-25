using System.Text.Json.Serialization;

namespace GithubBackup.Core.Tests.Flurl;

internal class Item(string id, ItemEnum itemEnum)
{
    [JsonPropertyName("id")]
    public string Id { get; } = id;

    [JsonPropertyName("itemEnum")]
    public ItemEnum ItemEnum { get; } = itemEnum;
}
