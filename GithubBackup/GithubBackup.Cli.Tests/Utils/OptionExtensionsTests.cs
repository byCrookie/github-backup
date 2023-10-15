using System.CommandLine;
using FluentAssertions;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Tests.Utils;

public class OptionExtensionsTests
{
    [Fact]
    public void GetRequiredValueForOption_HasValue_ReturnValue()
    {
        var rootCommand = new RootCommand();
        var option = new Option<string>("--option");
        rootCommand.AddOption(option);

        const string value = "value";
        const string args = $"--option {value}";
        
        var parseResult = rootCommand.Parse(args);
        var result = parseResult.GetRequiredValueForOption(option);
        
        result.Should().Be(value);
    }
    
    [Fact]
    public void GetRequiredValueForOption_DoesNotHaveValue_ThrowException()
    {
        var rootCommand = new RootCommand();
        var option = new Option<string>("--option");
        rootCommand.AddOption(option);
        
        var parseResult = rootCommand.Parse(string.Empty);
        var action = () => parseResult.GetRequiredValueForOption(option);

        action.Should().Throw<ArgumentNullException>();
    }
}