using System.CommandLine;
using System.CommandLine.Parsing;
using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Migrations;
using GithubBackup.Cli.Tests.Utils;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Tests.Commands.Github.Migrations;


public class MigrationsArgsTests
{
    private readonly MigrationsArguments _migrationsArguments = new();
    private readonly LoginArguments _loginArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassedWithDaysOld_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrationsArguments.Options());
        rootCommand.AddGlobalOptions(_loginArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            migrationsArgs =>
            {
                migrationsArgs.Should().NotBeNull();
                migrationsArgs.Export.Should().BeTrue();
                migrationsArgs.DaysOld.Should().Be(7);
                migrationsArgs.Since.Should().BeNull();
                migrationsArgs.LoginArgs.Token.Should().Be("test");
                migrationsArgs.LoginArgs.DeviceFlowAuth.Should().BeTrue();
            },
            new MigrationsArgsBinder(_migrationsArguments, _loginArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand)
            .InvokeAsync("sub --export --days-old 7 --token test --device-flow-auth");
    }
    
    [Fact]
    public async Task InvokeAsync_FlagsArePassedWithSince_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrationsArguments.Options());
        rootCommand.AddGlobalOptions(_loginArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            migrationsArgs =>
            {
                migrationsArgs.Should().NotBeNull();
                migrationsArgs.Export.Should().BeTrue();
                migrationsArgs.DaysOld.Should().BeNull();
                migrationsArgs.Since.Should().Be(new DateTime(2021, 1, 1));
                migrationsArgs.LoginArgs.Token.Should().BeNull();
                migrationsArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
            },
            new MigrationsArgsBinder(_migrationsArguments, _loginArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand)
            .InvokeAsync("sub --export --since 2021-01-01");
    }
    
    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassedWithDaysOld_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrationsArguments.Options());
        rootCommand.AddGlobalOptions(_loginArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            migrationsArgs =>
            {
                migrationsArgs.Should().NotBeNull();
                migrationsArgs.Export.Should().BeTrue();
                migrationsArgs.DaysOld.Should().Be(7);
                migrationsArgs.Since.Should().BeNull();
                migrationsArgs.LoginArgs.Token.Should().BeNull();
                migrationsArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
            },
            new MigrationsArgsBinder(_migrationsArguments, _loginArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand)
            .InvokeAsync("sub -e -d 7");
    }
    
    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassedWithSince_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrationsArguments.Options());
        rootCommand.AddGlobalOptions(_loginArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            migrationsArgs =>
            {
                migrationsArgs.Should().NotBeNull();
                migrationsArgs.Export.Should().BeTrue();
                migrationsArgs.DaysOld.Should().BeNull();
                migrationsArgs.Since.Should().Be(new DateTime(2021, 1, 1));
                migrationsArgs.LoginArgs.Token.Should().BeNull();
                migrationsArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
            },
            new MigrationsArgsBinder(_migrationsArguments, _loginArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand)
            .InvokeAsync("sub -e -s 2021-01-01");
    }
    
    [Fact]
    public async Task InvokeAsync_OnlyRequiredArePassed_FlagsGetParsedWithDefaults()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrationsArguments.Options());
        rootCommand.AddGlobalOptions(_loginArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            migrationsArgs =>
            {
                migrationsArgs.Should().NotBeNull();
                migrationsArgs.Export.Should().BeTrue();
                migrationsArgs.DaysOld.Should().BeNull();
                migrationsArgs.Since.Should().BeNull();
                migrationsArgs.LoginArgs.Token.Should().BeNull();
                migrationsArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
            },
            new MigrationsArgsBinder(_migrationsArguments, _loginArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub");
    }
    
    [Fact]
    public async Task InvokeAsync_SinceAndDaysOld_ValidationFails()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrationsArguments.Options());
        rootCommand.AddGlobalOptions(_loginArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            _ => { },
            new MigrationsArgsBinder(_migrationsArguments, _loginArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        var action = () => TestCommandline.Build(rootCommand).InvokeAsync("sub --since 2021-01-01 --days-old 7");
        
        await action.Should().ThrowAsync<Exception>();
    }
}