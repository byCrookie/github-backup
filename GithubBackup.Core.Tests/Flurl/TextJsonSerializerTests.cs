using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using GithubBackup.Core.Utils;

namespace GithubBackup.Core.Tests.Flurl;

public class TextJsonSerializerTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public async Task Serialize_WhenSerializing_ThenIsJson()
    {
        var item = new Item("test", ItemEnum.Test2);
        var json = JsonSerializer.Serialize(item, _options);
        await Verify(json);
    }

    [Fact]
    public void Deserialize_WhenDeserializing_ThenIsObject()
    {
        const string id = "test";
        var itemEnum = ItemEnum.Test2.GetEnumMemberValue();
        var json = $$"""{"id":"{{id}}","itemEnum":"{{itemEnum}}"}""";
        var item = JsonSerializer.Deserialize<Item>(json, _options);
        item.Should().NotBeNull();
        item!.Id.Should().Be("test");
        item.ItemEnum.Should().Be(ItemEnum.Test2);
    }
}
