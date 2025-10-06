using System.CommandLine;
using AwesomeAssertions;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Utils;
using GithubBackup.TestUtils;

namespace GithubBackup.Cli.Tests.Commands.Github.Migrate;

public class MigrateArgsTests
{
    private readonly MigrateArguments _migrateArguments = new(false);
    private readonly IntervalArguments _intervalArguments = new();
    private readonly LoginArguments _loginArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassed_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_migrateArguments.Options());
        command.AddOptions(_intervalArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var migrateArgs = new MigrateArgsBinder(_migrateArguments, _intervalArguments, _loginArguments).Get(p);
            migrateArgs.Should().NotBeNull();
            migrateArgs.Repositories.Should().BeEquivalentTo("repo1", "repo2");
            migrateArgs.LockRepositories.Should().BeTrue();
            migrateArgs.ExcludeMetadata.Should().BeTrue();
            migrateArgs.ExcludeGitData.Should().BeTrue();
            migrateArgs.ExcludeAttachements.Should().BeTrue();
            migrateArgs.ExcludeReleases.Should().BeTrue();
            migrateArgs.ExcludeOwnerProjects.Should().BeTrue();
            migrateArgs.OrgMetadataOnly.Should().BeFalse();
            migrateArgs.IntervalArgs.Interval.Should().Be(TimeSpan.FromSeconds(100));
            migrateArgs.LoginArgs.Token.Should().Be("test");
            migrateArgs.LoginArgs.DeviceFlowAuth.Should().BeTrue();
        });

        await command.Parse(
            "sub --repositories repo1 repo2 --lock-repositories --exclude-metadata"
            + " --exclude-git-data --exclude-attachements --exclude-releases --exclude-owner-projects --interval 100"
            + " --token test --device-flow-auth"
        ).InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassed_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_migrateArguments.Options());
        command.AddOptions(_intervalArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var migrateArgs = new MigrateArgsBinder(_migrateArguments, _intervalArguments, _loginArguments).Get(p);
            migrateArgs.Should().NotBeNull();
            migrateArgs.Repositories.Should().BeEquivalentTo("repo1", "repo2");
            migrateArgs.LockRepositories.Should().BeTrue();
            migrateArgs.ExcludeMetadata.Should().BeTrue();
            migrateArgs.ExcludeGitData.Should().BeTrue();
            migrateArgs.ExcludeAttachements.Should().BeTrue();
            migrateArgs.ExcludeReleases.Should().BeTrue();
            migrateArgs.ExcludeOwnerProjects.Should().BeTrue();
            migrateArgs.OrgMetadataOnly.Should().BeFalse();
            migrateArgs.IntervalArgs.Interval.Should().Be(TimeSpan.FromSeconds(100));
            migrateArgs.LoginArgs.Token.Should().BeNull();
            migrateArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
        });

        await command.Parse("sub -r repo1 repo2 -lr -em -egd -ea -er -eop -i 100").InvokeTestAsync();
    }

    [Theory]
    [InlineData("-r repo1 -r repo2")]
    [InlineData("-r repo1 repo2")]
    [InlineData("--repositories repo1 --repositories repo2")]
    [InlineData("--repositories repo1 repo2")]
    public async Task InvokeAsync_MigrationIsPassedMultipleTimes_FlagsGetParsed(
        string migrationArgs
    )
    {
        var command = new Command("sub");
        command.AddOptions(_migrateArguments.Options());
        command.AddOptions(_intervalArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var migrateArgs = new MigrateArgsBinder(_migrateArguments, _intervalArguments, _loginArguments).Get(p);
            migrateArgs.Should().NotBeNull();
            migrateArgs.Repositories.Should().BeEquivalentTo("repo1", "repo2");
            migrateArgs.LockRepositories.Should().BeFalse();
            migrateArgs.ExcludeMetadata.Should().BeFalse();
            migrateArgs.ExcludeGitData.Should().BeFalse();
            migrateArgs.ExcludeAttachements.Should().BeFalse();
            migrateArgs.ExcludeReleases.Should().BeFalse();
            migrateArgs.ExcludeOwnerProjects.Should().BeFalse();
            migrateArgs.OrgMetadataOnly.Should().BeFalse();
            migrateArgs.IntervalArgs.Interval.Should().BeNull();
            migrateArgs.LoginArgs.Token.Should().BeNull();
            migrateArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
        });

        await command.Parse("sub " + migrationArgs).InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_OnlyRequiredArePassed_FlagsGetParsedWithDefaults()
    {
        var command = new Command("sub");
        command.AddOptions(_migrateArguments.Options());
        command.AddOptions(_intervalArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var migrateArgs = new MigrateArgsBinder(_migrateArguments, _intervalArguments, _loginArguments).Get(p);
            migrateArgs.Should().NotBeNull();
            migrateArgs.Repositories.Should().BeEquivalentTo("repo1", "repo2");
            migrateArgs.LockRepositories.Should().BeFalse();
            migrateArgs.ExcludeMetadata.Should().BeFalse();
            migrateArgs.ExcludeGitData.Should().BeFalse();
            migrateArgs.ExcludeAttachements.Should().BeFalse();
            migrateArgs.ExcludeReleases.Should().BeFalse();
            migrateArgs.ExcludeOwnerProjects.Should().BeFalse();
            migrateArgs.OrgMetadataOnly.Should().BeFalse();
            migrateArgs.IntervalArgs.Interval.Should().BeNull();
            migrateArgs.LoginArgs.Token.Should().BeNull();
            migrateArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
        });

        await command.Parse("sub -r repo1 repo2").InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_OrgMetadataOnlyAndRepositories_ValidationFails()
    {
        var command = new Command("sub");
        command.AddOptions(_migrateArguments.Options());
        command.AddOptions(_intervalArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var migrateArgs = new MigrateArgsBinder(_migrateArguments, _intervalArguments, _loginArguments).Get(p);
            migrateArgs.Should().NotBeNull();
        });

        var action = () => command.Parse("sub --org-metadata-only --repositories repo1 repo2").InvokeTestAsync();

        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task InvokeAsync_NoRepositories_ValidationFails()
    {
        var command = new Command("sub");
        command.AddOptions(_migrateArguments.Options());
        command.AddOptions(_intervalArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var migrateArgs = new MigrateArgsBinder(_migrateArguments, _intervalArguments, _loginArguments).Get(p);
            migrateArgs.Should().NotBeNull();
        });

        var action = () => command.Parse("sub").InvokeTestAsync();
        await action.Should().ThrowAsync<Exception>();
    }
}