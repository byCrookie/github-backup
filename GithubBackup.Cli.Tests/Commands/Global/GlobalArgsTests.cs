using System.CommandLine;
using AwesomeAssertions;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;
using GithubBackup.TestUtils;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Tests.Commands.Global;

public class GlobalArgsTests
{
    private readonly GlobalArguments _globalArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassed_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_globalArguments.Options());

        command.SetAction(p =>
        {
            var globalArgs = new GlobalArgsBinder(_globalArguments).Get(p);
            globalArgs.Should().NotBeNull();
            globalArgs.LogFile!.Name.Should().Be("log.txt");
            globalArgs.Quiet.Should().BeTrue();
            globalArgs.Verbosity.Should().Be(LogLevel.Debug);
        });

        await command.Parse("sub --quiet --verbosity debug --log-file ./log.txt").InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassed_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_globalArguments.Options());

        command.SetAction(p =>
        {
            var globalArgs = new GlobalArgsBinder(_globalArguments).Get(p);
            globalArgs.Should().NotBeNull();
            globalArgs.LogFile!.Name.Should().Be("log.txt");
            globalArgs.Quiet.Should().BeTrue();
            globalArgs.Verbosity.Should().Be(LogLevel.Debug);
        });

        await command.Parse("sub -q -v debug -l ./log.txt").InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_NoFlagsArePassed_DefaultsAreUsed()
    {
        var command = new Command("sub");
        command.AddOptions(_globalArguments.Options());

        command.SetAction(p =>
        {
            var globalArgs = new GlobalArgsBinder(_globalArguments).Get(p);
            globalArgs.Should().NotBeNull();
            globalArgs.LogFile.Should().BeNull();
            globalArgs.Quiet.Should().BeTrue();
            globalArgs.Verbosity.Should().Be(LogLevel.Information);
        });

        await command.Parse("sub").InvokeTestAsync();
    }
}
