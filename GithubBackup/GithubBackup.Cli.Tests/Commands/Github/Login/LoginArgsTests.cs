using System.CommandLine;
using System.CommandLine.Parsing;
using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Tests.Utils;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Tests.Commands.Github.Login;

public class LoginArgsTests
{
    [Fact]
    public async Task InvokeAsync_FlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(LoginArgs.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            loginArgs =>
            {
                loginArgs.Should().NotBeNull();
                loginArgs.Token.Should().Be("token");
                loginArgs.DeviceFlowAuth.Should().BeTrue();
            },
            new LoginArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub --token token --device-flow-auth");
    }
    
    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(LoginArgs.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            loginArgs =>
            {
                loginArgs.Should().NotBeNull();
                loginArgs.Token.Should().Be("token");
                loginArgs.DeviceFlowAuth.Should().BeTrue();
            },
            new LoginArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub -t token -dfa");
    }
    
    [Fact]
    public async Task InvokeAsync_OnlyRequiredArePassed_FlagsGetParsedWithDefaults()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(LoginArgs.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            loginArgs =>
            {
                loginArgs.Should().NotBeNull();
                loginArgs.Token.Should().BeNull();
                loginArgs.DeviceFlowAuth.Should().BeFalse();
            },
            new LoginArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub");
    }
}

	