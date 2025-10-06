using AwesomeAssertions;
using GithubBackup.Core.Utils;

namespace GithubBackup.Core.Tests.Utils;

public class EnumExtensionsTests
{
    [Fact]
    public void GetEnumMemberValue_WhenHasAttribute_ThenReturnEnumMemberValue()
    {
        var result = TestEnum.Test1.GetEnumMemberValue();
        result.Should().Be("test1");
    }

    [Fact]
    public void GetEnumMemberValue_WhenHasNoAttribute_ThenReturnEnumValueName()
    {
        var result = TestEnum.Test2.GetEnumMemberValue();
        result.Should().Be("Test2");
    }
}
