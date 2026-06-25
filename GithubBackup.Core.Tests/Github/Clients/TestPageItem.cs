using System.Text.Json.Serialization;

namespace GithubBackup.Core.Tests.Github.Clients;

public class TestPageItem(int id, string name)
{
    [JsonPropertyName("id")]
    public int Id { get; set; } = id;

    [JsonPropertyName("name")]
    public string Name { get; set; } = name;
}
