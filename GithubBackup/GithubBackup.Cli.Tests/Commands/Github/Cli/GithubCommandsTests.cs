using System.CommandLine;
using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Cli;

namespace GithubBackup.Cli.Tests.Commands.Github.Cli;

public class GithubCommandsTests
{
    [Fact]
    public void AddCommands_Executed_SubCommandsAreAdded()
    {
        var args = Array.Empty<string>();
        var rootCommand = new RootCommand();
        GithubCommands.AddCommands(args, rootCommand);
        rootCommand.Subcommands.Should().HaveCount(7);
    }
}

	