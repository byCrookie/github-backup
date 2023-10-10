using FluentAssertions;
using GithubBackup.Core.Flurl;
using GithubBackup.Core.Utils;

namespace GithubBackup.Core.Tests.Flurl;

[UsesVerify]
public class TextJsonSerializerTests
{
    private readonly TextJsonSerializer _sut = new();

    [Fact]
    public void Serialize_WhenSerializing_ThenIsJson()
    {
        var item = new Item("test", ItemEnum.Test2);
        var json = _sut.Serialize(item);
        VerifyJson(json);
    }
    
    [Fact]
    public void Deserialize_WhenDeserializing_ThenIsObject()
    {
        const string id = "test";
        var itemEnum = ItemEnum.Test2.GetEnumMemberValue();
        var json = $$"""{"id":"{{id}}","itemEnum":"{{itemEnum}}"}""";
        var item = _sut.Deserialize<Item>(json);
        item.Should().NotBeNull();
        item!.Id.Should().Be("test");
        item.ItemEnum.Should().Be(ItemEnum.Test2);
    }
}