using System.CommandLine;
using System.CommandLine.Parsing;
using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Interval;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Tests.Utils;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Tests.Commands.Github.Migrate;

[UsesVerify]
public class MigrateArgsTests
{
    private readonly MigrateArguments _migrateArguments = new(false);
    private readonly IntervalArguments _intervalArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrateArguments.Options());
        rootCommand.AddGlobalOptions(_intervalArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            migrateArgs =>
            {
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
            },
            new MigrateArgsBinder(_migrateArguments, _intervalArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand)
            .InvokeAsync("sub --repositories repo1 repo2 --lock-repositories --exclude-metadata" +
                         " --exclude-git-data --exclude-attachements --exclude-releases --exclude-owner-projects --interval 100");
    }
    
    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrateArguments.Options());
        rootCommand.AddGlobalOptions(_intervalArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            migrateArgs =>
            {
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
            },
            new MigrateArgsBinder(_migrateArguments, _intervalArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub -r repo1 repo2 -lr -em -egd -ea -er -eop -i 100");
    }
    
    [Theory]
    [InlineData("-r repo1 -r repo2")]
    [InlineData("-r repo1 repo2")]
    [InlineData("--repositories repo1 --repositories repo2")]
    [InlineData("--repositories repo1 repo2")]
    public async Task InvokeAsync_MigrationIsPassedMultipleTimes_FlagsGetParsed(string migrationArgs)
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrateArguments.Options());
        rootCommand.AddGlobalOptions(_intervalArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            migrateArgs =>
            {
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
            },
            new MigrateArgsBinder(_migrateArguments, _intervalArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub " + migrationArgs);
    }
    
    [Fact]
    public async Task InvokeAsync_OnlyRequiredArePassed_FlagsGetParsedWithDefaults()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrateArguments.Options());
        rootCommand.AddGlobalOptions(_intervalArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            migrateArgs =>
            {
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
            },
            new MigrateArgsBinder(_migrateArguments, _intervalArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub -r repo1 repo2");
    }
    
    [Fact]
    public async Task InvokeAsync_OrgMetadataOnlyAndRepositories_ValidationFails()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrateArguments.Options());
        rootCommand.AddGlobalOptions(_intervalArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            _ => { },
            new MigrateArgsBinder(_migrateArguments, _intervalArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        var action = () => TestCommandline.Build(rootCommand).InvokeAsync("sub --org-metadata-only --repositories repo1 repo2");
        
        await action.Should().ThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task InvokeAsync_NoRepositories_ValidationFails()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_migrateArguments.Options());
        rootCommand.AddGlobalOptions(_intervalArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            _ => { },
            new MigrateArgsBinder(_migrateArguments, _intervalArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        var action = () => TestCommandline.Build(rootCommand).InvokeAsync("sub");
        
        await action.Should().ThrowAsync<Exception>();
    }
}