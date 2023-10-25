using System.CommandLine;
using System.CommandLine.Parsing;
using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Github.Migrations;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Tests.Utils;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Tests.Commands.Github.Migrations;

[UsesVerify]
public class MigrationsArgsTests
{
    private readonly MigrationsArguments _migrationsArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrationsArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            migrationsArgs =>
            {
                migrationsArgs.Should().NotBeNull();
                migrationsArgs.Long.Should().BeTrue();
            },
            new MigrationsArgsBinder(_migrationsArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand)
            .InvokeAsync("sub --long");
    }
    
    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrationsArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            migrationsArgs =>
            {
                migrationsArgs.Should().NotBeNull();
                migrationsArgs.Long.Should().BeTrue();
            },
            new MigrationsArgsBinder(_migrationsArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub -l");
    }
    
    [Fact]
    public async Task InvokeAsync_OnlyRequiredArePassed_FlagsGetParsedWithDefaults()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrationsArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            migrationsArgs =>
            {
                migrationsArgs.Should().NotBeNull();
                migrationsArgs.Long.Should().BeFalse();
            },
            new MigrationsArgsBinder(_migrationsArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub");
    }
}