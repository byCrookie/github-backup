using System.CommandLine;
using AwesomeAssertions;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Utils;
using GithubBackup.TestUtils;

namespace GithubBackup.Cli.Tests.Commands.Github.Download;

public class DownloadArgsTests
{
    private readonly DownloadArguments _downloadArguments = new(false);
    private readonly IntervalArguments _intervalArguments = new();
    private readonly LoginArguments _loginArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassed_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_downloadArguments.Options());
        command.AddOptions(_intervalArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var downloadArgs = new DowndloadArgsBinder(_downloadArguments, _intervalArguments, _loginArguments).Get(p);
            downloadArgs.Should().NotBeNull();
            downloadArgs.Destination.Name.Should().Be("migrations");
            downloadArgs.Latest.Should().BeTrue();
            downloadArgs.Migrations.Should().BeEquivalentTo(new long[] { 1, 2, 3 });
            downloadArgs.NumberOfBackups.Should().Be(5);
            downloadArgs.Overwrite.Should().BeTrue();
            downloadArgs.IntervalArgs.Interval.Should().Be(TimeSpan.FromSeconds(100));
            downloadArgs.LoginArgs.Token.Should().Be("test");
            downloadArgs.LoginArgs.DeviceFlowAuth.Should().BeTrue();
        });

        await command.Parse(
                "sub --destination ./migrations --latest --migrations 1 2 3"
                    + " --number-of-backups 5 --overwrite --interval 100"
                    + " --token test --device-flow-auth"
            ).InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassed_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_downloadArguments.Options());
        command.AddOptions(_intervalArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var downloadArgs = new DowndloadArgsBinder(_downloadArguments, _intervalArguments, _loginArguments).Get(p);
            downloadArgs.Should().NotBeNull();
            downloadArgs.Destination.Name.Should().Be("migrations");
            downloadArgs.Latest.Should().BeTrue();
            downloadArgs.Migrations.Should().BeEquivalentTo(new long[] { 1, 2, 3 });
            downloadArgs.NumberOfBackups.Should().Be(5);
            downloadArgs.Overwrite.Should().BeTrue();
            downloadArgs.IntervalArgs.Interval.Should().Be(TimeSpan.FromSeconds(100));
            downloadArgs.LoginArgs.Token.Should().BeNull();
            downloadArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
        });

        await command.Parse("sub -d ./migrations -l -m 1 2 3 -n 5 -o -i 100").InvokeTestAsync();
    }

    [Theory]
    [InlineData("-m 1 -m 2 -m 3")]
    [InlineData("-m 1 2 3")]
    [InlineData("--migrations 1 --migrations 2 --migrations 3")]
    [InlineData("--migrations 1 2 3")]
    public async Task InvokeAsync_MigrationIsPassedMultipleTimes_FlagsGetParsed(
        string migrationArgs
    )
    {
        var command = new Command("sub");
        command.AddOptions(_downloadArguments.Options());
        command.AddOptions(_intervalArguments.Options());

        command.SetAction(p =>
        {
            var downloadArgs = new DowndloadArgsBinder(_downloadArguments, _intervalArguments, _loginArguments).Get(p);
            downloadArgs.Should().NotBeNull();
            downloadArgs.Destination.Name.Should().Be("migrations");
            downloadArgs.Latest.Should().BeTrue();
            downloadArgs.Migrations.Should().BeEquivalentTo(new long[] { 1, 2, 3 });
            downloadArgs.NumberOfBackups.Should().Be(5);
            downloadArgs.Overwrite.Should().BeTrue();
            downloadArgs.IntervalArgs.Interval.Should().BeNull();
            downloadArgs.LoginArgs.Token.Should().BeNull();
            downloadArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
        });

        await command.Parse("sub -d ./migrations -l -o -n 5 " + migrationArgs).InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_OnlyRequiredArePassed_FlagsGetParsedWithDefaults()
    {
        var command = new Command("sub");
        command.AddOptions(_downloadArguments.Options());
        command.AddOptions(_intervalArguments.Options());

        command.SetAction(p =>
        {
            var downloadArgs = new DowndloadArgsBinder(_downloadArguments, _intervalArguments, _loginArguments).Get(p);
            downloadArgs.Should().NotBeNull();
            downloadArgs.Destination.Name.Should().Be("migrations");
            downloadArgs.Latest.Should().BeFalse();
            downloadArgs.Migrations.Should().BeEmpty();
            downloadArgs.NumberOfBackups.Should().BeNull();
            downloadArgs.Overwrite.Should().BeTrue();
            downloadArgs.IntervalArgs.Interval.Should().BeNull();
            downloadArgs.LoginArgs.Token.Should().BeNull();
            downloadArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
        });

        await command.Parse("sub -d ./migrations").InvokeTestAsync();
    }
}
