using System.CommandLine;
using System.CommandLine.Parsing;
using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Github.Repositories;
using GithubBackup.Cli.Tests.Utils;
using GithubBackup.Cli.Utils;
using GithubBackup.Core.Github.Repositories;

namespace GithubBackup.Cli.Tests.Commands.Github.Repositories;

[UsesVerify]
public class RepositoriesArgsTests
{
    private readonly RepositoriesArguments _repositoriesArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassedWithType_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_repositoriesArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            repositoriesArgs =>
            {
                repositoriesArgs.Should().NotBeNull();
                repositoriesArgs.Type.Should().Be(RepositoryType.Public);
                repositoriesArgs.Affiliation.Should().Be(RepositoryAffiliation.Owner);
                repositoriesArgs.Visibility.Should().Be(RepositoryVisibility.All);
            },
            new RepositoriesArgsBinder(_repositoriesArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand).InvokeAsync("sub --type public");
    }
    
    [Fact]
    public async Task InvokeAsync_FlagsArePassedWithAffiliationAndVisibility_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_repositoriesArguments.Options());
        var subCommand = new Command("sub");
        
        subCommand.SetHandler(
            repositoriesArgs =>
            {
                repositoriesArgs.Should().NotBeNull();
                repositoriesArgs.Type.Should().BeNull();
                repositoriesArgs.Affiliation.Should().Be(RepositoryAffiliation.Collaborator);
                repositoriesArgs.Visibility.Should().Be(RepositoryVisibility.Private);
            },
            new RepositoriesArgsBinder(_repositoriesArguments)
        );
        
        rootCommand.AddCommand(subCommand);
        await TestCommandline.Build(rootCommand)
            .InvokeAsync("sub --affiliation collaborator --visibility private");
    }
}