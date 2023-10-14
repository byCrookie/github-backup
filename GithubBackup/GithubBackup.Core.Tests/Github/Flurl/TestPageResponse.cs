using System.Text.Json.Serialization;

namespace GithubBackup.Core.Tests.Github.Flurl;

public class TestPageResponse
{
    [JsonPropertyName("items")]
    public List<TestPageItem> Items { get; set; }

    public TestPageResponse(List<TestPageItem> items)
    {
        Items = items;
    }
}