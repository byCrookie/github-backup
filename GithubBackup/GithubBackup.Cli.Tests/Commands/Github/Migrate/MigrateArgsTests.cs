﻿using System.CommandLine;
using System.CommandLine.Parsing;
using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Tests.Utils;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Tests.Commands.Github.Migrate;

[UsesVerify]
public class MigrateArgsTests
{
    public MigrateArgsTests()
    {
        Piping.IsEnabled = false;
    }
    
    [Fact]
    public async Task InvokeAsync_FlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(MigrateArgs.Options());
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
            },
            new MigrateArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand)
            .InvokeAsync("sub --repositories repo1 repo2 --lock-repositories --exclude-metadata" +
                         " --exclude-git-data --exclude-attachements --exclude-releases --exclude-owner-projects");
    }
    
    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(MigrateArgs.Options());
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
            },
            new MigrateArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub -r repo1 repo2 -lr -em -egd -ea -er -eop");
    }
    
    [Theory]
    [InlineData("-r repo1 -r repo2")]
    [InlineData("-r repo1 repo2")]
    [InlineData("--repositories repo1 --repositories repo2")]
    [InlineData("--repositories repo1 repo2")]
    public async Task InvokeAsync_MigrationIsPassedMultipleTimes_FlagsGetParsed(string migrationArgs)
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(MigrateArgs.Options());
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
            },
            new MigrateArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub " + migrationArgs);
    }
    
    [Fact]
    public async Task InvokeAsync_OnlyRequiredArePassed_FlagsGetParsedWithDefaults()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(MigrateArgs.Options());
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
            },
            new MigrateArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub -r repo1 repo2");
    }
    
    [Fact]
    public async Task InvokeAsync_OrgMetadataOnlyAndRepositories_ValidationFails()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(MigrateArgs.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            _ => { },
            new MigrateArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        var action = () => TestCommandline.Build(rootCommand).InvokeAsync("sub --org-metadata-only --repositories repo1 repo2");
        
        await action.Should().ThrowAsync<Exception>();
    }
}