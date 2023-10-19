using System.CommandLine;
using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Tests.Commands.Github.Download;

public class DownloadArgsTests
{
    [Fact]
    public async Task InvokeAsync_FlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(DownloadArgs.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            downloadArgs =>
            {
                downloadArgs.Should().NotBeNull();
                downloadArgs.Destination.Should().Be("./migrations");
                downloadArgs.Latest.Should().BeTrue();
                downloadArgs.Migrations.Should().BeEquivalentTo(new long[] { 1, 2, 3 });
                downloadArgs.NumberOfBackups.Should().Be(5);
                downloadArgs.Overwrite.Should().BeTrue();
            },
            new DowndloadArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        await rootCommand.InvokeAsync("sub --destination ./migrations --latest --migrations 1 2 3 --number-of-backups 5 --overwrite");
    }
    
    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(DownloadArgs.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            downloadArgs =>
            {
                downloadArgs.Should().NotBeNull();
                downloadArgs.Destination.Should().Be("./migrations");
                downloadArgs.Latest.Should().BeTrue();
                downloadArgs.Migrations.Should().BeEquivalentTo(new long[] { 1, 2, 3 });
                downloadArgs.NumberOfBackups.Should().Be(5);
                downloadArgs.Overwrite.Should().BeTrue();
            },
            new DowndloadArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        await rootCommand.InvokeAsync("sub -d ./migrations -l -m 1 2 3 -n 5 -o");
    }
    
    [Theory]
    [InlineData("-m 1 -m 2 -m 3")]
    [InlineData("-m 1 2 3")]
    [InlineData("--migrations 1 --migrations 2 --migrations 3")]
    [InlineData("--migrations 1 2 3")]
    public async Task InvokeAsync_MigrationIsPassedMultipleTimes_FlagsGetParsed(string migrationArgs)
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(DownloadArgs.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            downloadArgs =>
            {
                downloadArgs.Should().NotBeNull();
                downloadArgs.Destination.Should().Be("./migrations");
                downloadArgs.Latest.Should().BeTrue();
                downloadArgs.Migrations.Should().BeEquivalentTo(new long[] { 1, 2, 3 });
                downloadArgs.NumberOfBackups.Should().Be(5);
                downloadArgs.Overwrite.Should().BeTrue();
            },
            new DowndloadArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        await rootCommand.InvokeAsync("sub -d ./migrations -l -o -n 5 " + migrationArgs);
    }
    
    [Fact]
    public async Task InvokeAsync_OnlyRequiredArePassed_FlagsGetParsedWithDefaults()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(DownloadArgs.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            downloadArgs =>
            {
                downloadArgs.Should().NotBeNull();
                downloadArgs.Destination.Should().Be("./migrations");
                downloadArgs.Latest.Should().BeFalse();
                downloadArgs.Migrations.Should().BeEquivalentTo(new long[] { 1, 2, 3 });
                downloadArgs.NumberOfBackups.Should().BeNull();
                downloadArgs.Overwrite.Should().BeTrue();
            },
            new DowndloadArgsBinder()
        );
        
        rootCommand.AddCommand(subCommand);
        await rootCommand.InvokeAsync("sub -d ./migrations -m 1 2 3");
    }
}

	