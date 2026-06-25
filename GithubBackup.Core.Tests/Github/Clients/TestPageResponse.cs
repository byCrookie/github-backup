using System.Text.Json.Serialization;

namespace GithubBackup.Core.Tests.Github.Clients;

public class TestPageResponse(List<TestPageItem> items)
{
    [JsonPropertyName("items")]
    public List<TestPageItem> Items { get; set; } = items;
}
