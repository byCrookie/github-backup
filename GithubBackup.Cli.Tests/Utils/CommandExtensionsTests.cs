﻿using System.CommandLine;
using FluentAssertions;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Tests.Utils;

public class CommandExtensionsTests
{
    [Fact]
    public void AddOptions_MultipleOptions_OptionsAreAdded()
    {
        var rootCommand = new RootCommand();
        var option1 = new Option<string>("--option1");
        var option2 = new Option<string>("--option2");

        rootCommand.AddOptions(new List<Option> { option1, option2 });

        rootCommand.Children.Should().Contain(option1);
        rootCommand.Children.Should().Contain(option2);
    }

    [Fact]
    public void AddGlobalOptions_MultipleOptions_GlobalOptionsAreAdded()
    {
        var rootCommand = new RootCommand();
        var option1 = new Option<string>("--option1");
        var option2 = new Option<string>("--option2");

        rootCommand.AddGlobalOptions(new List<Option> { option1, option2 });

        rootCommand.Children.Should().Contain(option1);
        rootCommand.Children.Should().Contain(option2);
    }

    [Fact]
    public void AddCommands_MultipleCommands_CommandsAreAdded()
    {
        var rootCommand = new RootCommand();
        var command1 = new Command("command1");
        var command2 = new Command("command2");

        rootCommand.AddCommands(new List<Command> { command1, command2 });

        rootCommand.Subcommands.Should().Contain(command1);
        rootCommand.Subcommands.Should().Contain(command2);
    }
}
