using FluentAssertions;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Tests;

public class ListExtensionsTests
{
    [Fact]
    public void AddAll_AddItems_ItemAreAdded()
    {
        ICollection<string> list = new List<string>
        {
            "1"
        };

        list.AddAll(new[] { "2", "3", "4" });
        
        list.Should().BeEquivalentTo(new List<string>
        {
            "1",
            "2",
            "3",
            "4"
        });
    }
}