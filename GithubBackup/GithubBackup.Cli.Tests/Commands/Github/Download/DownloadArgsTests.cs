using System.CommandLine;
using System.CommandLine.Parsing;
using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Tests.Utils;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Tests.Commands.Github.Download;

public class DownloadArgsTests
{
    private readonly DownloadArguments _downloadArguments = new(false);
    private readonly IntervalArguments _intervalArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_downloadArguments.Options());
        rootCommand.AddGlobalOptions(_intervalArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            downloadArgs =>
            {
                downloadArgs.Should().NotBeNull();
                downloadArgs.Destination.Name.Should().Be("migrations");
                downloadArgs.Latest.Should().BeTrue();
                downloadArgs.Migrations.Should().BeEquivalentTo(new long[] { 1, 2, 3 });
                downloadArgs.NumberOfBackups.Should().Be(5);
                downloadArgs.Overwrite.Should().BeTrue();
                downloadArgs.IntervalArgs.Interval.Should().Be(TimeSpan.FromSeconds(100));
            },
            new DowndloadArgsBinder(_downloadArguments, _intervalArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub --destination ./migrations --latest --migrations 1 2 3 --number-of-backups 5 --overwrite --interval 100");
    }
    
    [Fact]
    public async Task InvokeAsync_ShortFlagsArePassed_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_downloadArguments.Options());
        rootCommand.AddGlobalOptions(_intervalArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            downloadArgs =>
            {
                downloadArgs.Should().NotBeNull();
                downloadArgs.Destination.Name.Should().Be("migrations");
                downloadArgs.Latest.Should().BeTrue();
                downloadArgs.Migrations.Should().BeEquivalentTo(new long[] { 1, 2, 3 });
                downloadArgs.NumberOfBackups.Should().Be(5);
                downloadArgs.Overwrite.Should().BeTrue();
                downloadArgs.IntervalArgs.Interval.Should().Be(TimeSpan.FromSeconds(100));
            },
            new DowndloadArgsBinder(_downloadArguments, _intervalArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub -d ./migrations -l -m 1 2 3 -n 5 -o -i 100");
    }
    
    [Theory]
    [InlineData("-m 1 -m 2 -m 3")]
    [InlineData("-m 1 2 3")]
    [InlineData("--migrations 1 --migrations 2 --migrations 3")]
    [InlineData("--migrations 1 2 3")]
    public async Task InvokeAsync_MigrationIsPassedMultipleTimes_FlagsGetParsed(string migrationArgs)
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_downloadArguments.Options());
        rootCommand.AddGlobalOptions(_intervalArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            downloadArgs =>
            {
                downloadArgs.Should().NotBeNull();
                downloadArgs.Destination.Name.Should().Be("migrations");
                downloadArgs.Latest.Should().BeTrue();
                downloadArgs.Migrations.Should().BeEquivalentTo(new long[] { 1, 2, 3 });
                downloadArgs.NumberOfBackups.Should().Be(5);
                downloadArgs.Overwrite.Should().BeTrue();
                downloadArgs.IntervalArgs.Interval.Should().BeNull();
            },
            new DowndloadArgsBinder(_downloadArguments, _intervalArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub -d ./migrations -l -o -n 5 " + migrationArgs);
    }
    
    [Fact]
    public async Task InvokeAsync_OnlyRequiredArePassed_FlagsGetParsedWithDefaults()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_downloadArguments.Options());
        rootCommand.AddGlobalOptions(_intervalArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            downloadArgs =>
            {
                downloadArgs.Should().NotBeNull();
                downloadArgs.Destination.Name.Should().Be("migrations");
                downloadArgs.Latest.Should().BeFalse();
                downloadArgs.Migrations.Should().BeEmpty();
                downloadArgs.NumberOfBackups.Should().BeNull();
                downloadArgs.Overwrite.Should().BeTrue();
                downloadArgs.IntervalArgs.Interval.Should().BeNull();
            },
            new DowndloadArgsBinder(_downloadArguments, _intervalArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub -d ./migrations");
    }
}

	