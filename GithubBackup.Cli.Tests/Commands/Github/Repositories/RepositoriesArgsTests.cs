using System.CommandLine;
using System.CommandLine.Parsing;
using FluentAssertions;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Repositories;
using GithubBackup.Cli.Tests.Utils;
using GithubBackup.Cli.Utils;
using GithubBackup.Core.Github.Repositories;

namespace GithubBackup.Cli.Tests.Commands.Github.Repositories;

public class RepositoriesArgsTests
{
    private readonly RepositoriesArguments _repositoriesArguments = new();
    private readonly LoginArguments _loginArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassedWithType_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_repositoriesArguments.Options());
        rootCommand.AddGlobalOptions(_loginArguments.Options());
        var subCommand = new Command("sub");

        subCommand.SetHandler(
            repositoriesArgs =>
            {
                repositoriesArgs.Should().NotBeNull();
                repositoriesArgs.Type.Should().Be(RepositoryType.Public);
                repositoriesArgs.Affiliation.Should().Be(RepositoryAffiliation.Owner);
                repositoriesArgs.Visibility.Should().Be(RepositoryVisibility.All);
                repositoriesArgs.LoginArgs.Token.Should().Be("test");
                repositoriesArgs.LoginArgs.DeviceFlowAuth.Should().BeTrue();
            },
            new RepositoriesArgsBinder(_repositoriesArguments, _loginArguments)
        );

        rootCommand.AddCommand(subCommand);
        await TestCommandline
            .Build(rootCommand)
            .InvokeAsync("sub --type public --token test --device-flow-auth");
    }

    [Fact]
    public async Task InvokeAsync_FlagsArePassedWithAffiliationAndVisibility_FlagsGetParsed()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOptions(_repositoriesArguments.Options());
        rootCommand.AddGlobalOptions(_loginArguments.Options());
        var subCommand = new Command("sub");

        subCommand.SetHandler(
            repositoriesArgs =>
            {
                repositoriesArgs.Should().NotBeNull();
                repositoriesArgs.Type.Should().BeNull();
                repositoriesArgs.Affiliation.Should().Be(RepositoryAffiliation.Collaborator);
                repositoriesArgs.Visibility.Should().Be(RepositoryVisibility.Private);
                repositoriesArgs.LoginArgs.Token.Should().BeNull();
                repositoriesArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
            },
            new RepositoriesArgsBinder(_repositoriesArguments, _loginArguments)
        );

        rootCommand.AddCommand(subCommand);
        await TestCommandline
            .Build(rootCommand)
            .InvokeAsync("sub --affiliation collaborator --visibility private");
    }
}
