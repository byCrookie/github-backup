using System.Text.Json.Serialization;

namespace GithubBackup.Core.Tests.Flurl;

internal class ItemResponse(List<Item> items)
{
    [JsonPropertyName("items")]
    public List<Item> Items { get; } = items;
}
