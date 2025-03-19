using System.CommandLine;
using System.CommandLine.Parsing;
using FluentAssertions;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Tests.Utils;
using GithubBackup.Cli.Utils;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Tests.Commands.Global;

public class GlobalArgsTests
{
    private readonly GlobalArguments _globalArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_globalArguments.Options());
        var subCommand = new Command("sub");

        subCommand.SetHandler(
            globalArgs =>
            {
                globalArgs.Should().NotBeNull();
                globalArgs.LogFile!.Name.Should().Be("log.txt");
                globalArgs.Quiet.Should().BeTrue();
                globalArgs.Verbosity.Should().Be(LogLevel.Debug);
            },
            new GlobalArgsBinder(_globalArguments)
        );

        rootCommand.AddCommand(subCommand);
        await TestCommandline
            .Build(rootCommand)
            .InvokeAsync("sub --quiet --verbosity debug --log-file ./log.txt");
    }

    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_globalArguments.Options());
        var subCommand = new Command("sub");

        subCommand.SetHandler(
            globalArgs =>
            {
                globalArgs.Should().NotBeNull();
                globalArgs.LogFile.Should().BeNull();
                globalArgs.Quiet.Should().BeTrue();
                globalArgs.Verbosity.Should().Be(LogLevel.Information);
            },
            new GlobalArgsBinder(_globalArguments)
        );

        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub -q -v debug -l ./log.txt");
    }

    [Fact]
    public async Task InvokeAsync_NoFlagsArePassed_DefaultsAreUsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_globalArguments.Options());
        var subCommand = new Command("sub");

        subCommand.SetHandler(
            globalArgs =>
            {
                globalArgs.Should().NotBeNull();
                globalArgs.LogFile.Should().BeNull();
                globalArgs.Quiet.Should().BeTrue();
                globalArgs.Verbosity.Should().Be(LogLevel.Information);
            },
            new GlobalArgsBinder(_globalArguments)
        );

        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub");
    }
}
