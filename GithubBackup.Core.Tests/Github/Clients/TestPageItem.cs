using System.Text.Json.Serialization;

namespace GithubBackup.Core.Tests.Github.Clients;

public class TestPageItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    public TestPageItem(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
