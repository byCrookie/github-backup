using System.CommandLine;
using AwesomeAssertions;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Migrations;
using GithubBackup.Cli.Utils;
using GithubBackup.TestUtils;

namespace GithubBackup.Cli.Tests.Commands.Github.Migrations;

public class MigrationsArgsTests
{
    private readonly MigrationsArguments _migrationsArguments = new();
    private readonly LoginArguments _loginArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassedWithDaysOld_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_migrationsArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var migrationsArgs = new MigrationsArgsBinder(_migrationsArguments, _loginArguments).Get(p);
            migrationsArgs.Should().NotBeNull();
            migrationsArgs.Export.Should().BeTrue();
            migrationsArgs.DaysOld.Should().Be(7);
            migrationsArgs.Since.Should().BeNull();
            migrationsArgs.LoginArgs.Token.Should().Be("test");
            migrationsArgs.LoginArgs.DeviceFlowAuth.Should().BeTrue();
        });

        await command.Parse("sub --export --days-old 7 --token test --device-flow-auth").InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_FlagsArePassedWithSince_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_migrationsArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var migrationsArgs = new MigrationsArgsBinder(_migrationsArguments, _loginArguments).Get(p);
            migrationsArgs.Should().NotBeNull();
            migrationsArgs.Export.Should().BeTrue();
            migrationsArgs.DaysOld.Should().BeNull();
            migrationsArgs.Since.Should().Be(new DateTime(2021, 1, 1));
            migrationsArgs.LoginArgs.Token.Should().BeNull();
            migrationsArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
        });

        await command.Parse("sub --export --since 2021-01-01").InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassedWithDaysOld_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_migrationsArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var migrationsArgs = new MigrationsArgsBinder(_migrationsArguments, _loginArguments).Get(p);
            migrationsArgs.Should().NotBeNull();
            migrationsArgs.Export.Should().BeTrue();
            migrationsArgs.DaysOld.Should().Be(7);
            migrationsArgs.Since.Should().BeNull();
            migrationsArgs.LoginArgs.Token.Should().BeNull();
            migrationsArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
        });

        await command.Parse("sub -e -d 7").InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassedWithSince_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_migrationsArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var migrationsArgs = new MigrationsArgsBinder(_migrationsArguments, _loginArguments).Get(p);
            migrationsArgs.Should().NotBeNull();
            migrationsArgs.Export.Should().BeTrue();
            migrationsArgs.DaysOld.Should().BeNull();
            migrationsArgs.Since.Should().Be(new DateTime(2021, 1, 1));
            migrationsArgs.LoginArgs.Token.Should().BeNull();
            migrationsArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
        });

        await command.Parse("sub -e -s 2021-01-01").InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_OnlyRequiredArePassed_FlagsGetParsedWithDefaults()
    {
        var command = new Command("sub");
        command.AddOptions(_migrationsArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var migrationsArgs = new MigrationsArgsBinder(_migrationsArguments, _loginArguments).Get(p);
            migrationsArgs.Should().NotBeNull();
            migrationsArgs.Export.Should().BeTrue();
            migrationsArgs.DaysOld.Should().BeNull();
            migrationsArgs.Since.Should().BeNull();
            migrationsArgs.LoginArgs.Token.Should().BeNull();
            migrationsArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
        });

        await command.Parse("sub").InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_SinceAndDaysOld_ValidationFails()
    {
        var command = new Command("sub");
        command.AddOptions(_migrationsArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var migrationsArgs = new MigrationsArgsBinder(_migrationsArguments, _loginArguments).Get(p);
            migrationsArgs.Should().NotBeNull();
        });

        var action = () => command.Parse("sub --since 2021-01-01 --days-old 7").InvokeTestAsync();

        await action.Should().ThrowAsync<Exception>();
    }
}
