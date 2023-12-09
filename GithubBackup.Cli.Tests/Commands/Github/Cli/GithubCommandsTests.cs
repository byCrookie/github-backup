using System.CommandLine;
using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Global;

namespace GithubBackup.Cli.Tests.Commands.Github.Cli;

public class GithubCommandsTests
{
    [Fact]
    public void AddCommands_Executed_SubCommandsAreAdded()
    {
        var args = Array.Empty<string>();
        var rootCommand = new RootCommand();
        var globalArguments = new GlobalArguments();
        GithubCommands.AddCommands(args, rootCommand, new CommandOptions
        {
            AfterServices = _ => { },
            GlobalArguments = globalArguments
        });
        rootCommand.Subcommands.Should().HaveCount(7);
    }
}