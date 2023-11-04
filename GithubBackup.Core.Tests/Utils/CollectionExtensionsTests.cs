using FluentAssertions;
using Flurl.Util;
using GithubBackup.Core.Utils;

namespace GithubBackup.Core.Tests.Utils;

public class CollectionExtensionsTests
{
    [Fact]
    public void Get_WhenHasKey_ThenValue()
    {
        const string key = "key";
        const string value = "value";
        var collection = new NameValueList<string>(true) { { key, value } };
        collection.Get(key).Should().Be(value);
        collection.GetRequired(key).Should().Be(value);
    }

    [Fact]
    public void Get_WhenHasNoKey_ThenReturnNull()
    {
        var collection = new NameValueList<string>(true);
        collection.Get("key").Should().BeNull();
    }

    [Fact]
    public void GetRequired_WhenHasNoKey_ThenThrowException()
    {
        var collection = new NameValueList<string>(true);
        Action action = () => collection.GetRequired("key");
        action.Should().Throw<InvalidOperationException>();
    }
}