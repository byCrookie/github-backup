using System.CommandLine;
using AwesomeAssertions;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Utils;
using GithubBackup.TestUtils;

namespace GithubBackup.Cli.Tests.Commands.Interval;

public class IntervalArgsTests
{
    private readonly IntervalArguments _intervalArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassed_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_intervalArguments.Options());
        
        command.SetAction(p =>
        {
            var intervalArgs = new IntervalArgsBinder(_intervalArguments).Get(p);
            intervalArgs.Should().NotBeNull();
            intervalArgs.Interval.Should().Be(TimeSpan.FromSeconds(10));
        });
        
        await command.Parse("sub --interval 10").InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassed_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_intervalArguments.Options());

        command.SetAction(p =>
        {
            var intervalArgs = new IntervalArgsBinder(_intervalArguments).Get(p);
            intervalArgs.Should().NotBeNull();
            intervalArgs.Interval.Should().Be(TimeSpan.FromSeconds(10));
        });
        
        await command.Parse("sub -i 10").InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_NoFlagsArePassed_DefaultsAreUsed()
    {
        var command = new Command("sub");
        command.AddOptions(_intervalArguments.Options());

        command.SetAction(p =>
        {
            var intervalArgs = new IntervalArgsBinder(_intervalArguments).Get(p);
            intervalArgs.Should().NotBeNull();
            intervalArgs.Interval.Should().BeNull();
        });

        await command.Parse("sub").InvokeTestAsync();
    }
}
