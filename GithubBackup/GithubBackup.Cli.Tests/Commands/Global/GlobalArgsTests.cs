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
    [Fact]
    public async Task InvokeAsync_FlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(GlobalArgs.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            globalArgs =>
            {
                globalArgs.Should().NotBeNull();
                globalArgs.Interactive.Should().BeTrue();
                globalArgs.LogFile!.Name.Should().Be("log.txt");
                globalArgs.Quiet.Should().BeTrue();
                globalArgs.Verbosity.Should().Be(LogLevel.Debug);
            },
            new GlobalArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub --quiet --verbosity debug --log-file ./log.txt --interactive");
    }
    
    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(GlobalArgs.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            globalArgs =>
            {
                globalArgs.Should().NotBeNull();
                globalArgs.Interactive.Should().BeTrue();
                globalArgs.LogFile!.Name.Should().Be("log.txt");
                globalArgs.Quiet.Should().BeTrue();
                globalArgs.Verbosity.Should().Be(LogLevel.Debug);
            },
            new GlobalArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub -q -v debug -l ./log.txt -i");
    }
    
    [Fact]
    public async Task InvokeAsync_NoFlagsArePassed_DefaultsAreUsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(GlobalArgs.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            globalArgs =>
            {
                globalArgs.Should().NotBeNull();
                globalArgs.Interactive.Should().BeFalse();
                globalArgs.LogFile.Should().BeNull();
                globalArgs.Quiet.Should().BeFalse();
                globalArgs.Verbosity.Should().Be(LogLevel.Information);
            },
            new GlobalArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub");
    }
}

	