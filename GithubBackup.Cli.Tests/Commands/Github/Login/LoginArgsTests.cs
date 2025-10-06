using System.CommandLine;
using AwesomeAssertions;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Utils;
using GithubBackup.TestUtils;

namespace GithubBackup.Cli.Tests.Commands.Github.Login;

public class LoginArgsTests
{
    private readonly LoginArguments _loginArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassed_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var loginArgs = new LoginArgsBinder(_loginArguments).Get(p);
            loginArgs.Should().NotBeNull();
            loginArgs.Token.Should().Be("token");
            loginArgs.DeviceFlowAuth.Should().BeTrue();
        });

        await command.Parse("sub --token token --device-flow-auth").InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassed_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var loginArgs = new LoginArgsBinder(_loginArguments).Get(p);
            loginArgs.Should().NotBeNull();
            loginArgs.Token.Should().Be("token");
            loginArgs.DeviceFlowAuth.Should().BeTrue();
        });

        await command.Parse("sub -t token -d").InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_OnlyRequiredArePassed_FlagsGetParsedWithDefaults()
    {
        var command = new Command("sub");
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var loginArgs = new LoginArgsBinder(_loginArguments).Get(p);
            loginArgs.Should().NotBeNull();
            loginArgs.Token.Should().BeNull();
            loginArgs.DeviceFlowAuth.Should().BeFalse();
        });

        await command.Parse("sub").InvokeTestAsync();
    }
}
