using System.CommandLine;
using System.CommandLine.Parsing;
using FluentAssertions;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Tests.Utils;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Tests.Commands.Interval;

public class IntervalArgsTests
{
    private readonly IntervalArguments _intervalArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_intervalArguments.Options());
        var subCommand = new Command("sub");

        subCommand.SetHandler(
            intervalArgs =>
            {
                intervalArgs.Should().NotBeNull();
                intervalArgs.Interval.Should().Be(TimeSpan.FromSeconds(10));
            },
            new IntervalArgsBinder(_intervalArguments)
        );

        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub --interval 10");
    }

    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_intervalArguments.Options());
        var subCommand = new Command("sub");

        subCommand.SetHandler(
            intervalArgs =>
            {
                intervalArgs.Should().NotBeNull();
                intervalArgs.Interval.Should().Be(TimeSpan.FromSeconds(10));
            },
            new IntervalArgsBinder(_intervalArguments)
        );

        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub -i 10");
    }

    [Fact]
    public async Task InvokeAsync_NoFlagsArePassed_DefaultsAreUsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_intervalArguments.Options());
        var subCommand = new Command("sub");

        subCommand.SetHandler(
            intervalArgs =>
            {
                intervalArgs.Should().NotBeNull();
                intervalArgs.Interval.Should().BeNull();
            },
            new IntervalArgsBinder(_intervalArguments)
        );

        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub");
    }
}
