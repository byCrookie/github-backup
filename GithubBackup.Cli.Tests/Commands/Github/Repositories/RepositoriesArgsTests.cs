using System.CommandLine;
using AwesomeAssertions;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Repositories;
using GithubBackup.Cli.Utils;
using GithubBackup.Core.Github.Repositories;
using GithubBackup.TestUtils;

namespace GithubBackup.Cli.Tests.Commands.Github.Repositories;

public class RepositoriesArgsTests
{
    private readonly RepositoriesArguments _repositoriesArguments = new();
    private readonly LoginArguments _loginArguments = new();

    [Fact]
    public async Task InvokeAsync_FlagsArePassedWithType_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_repositoriesArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var repositoriesArgs = new RepositoriesArgsBinder(_repositoriesArguments, _loginArguments).Get(p);
            repositoriesArgs.Should().NotBeNull();
            repositoriesArgs.Type.Should().Be(RepositoryType.Public);
            repositoriesArgs.Affiliation.Should().Be(RepositoryAffiliation.Owner);
            repositoriesArgs.Visibility.Should().Be(RepositoryVisibility.All);
            repositoriesArgs.LoginArgs.Token.Should().Be("test");
            repositoriesArgs.LoginArgs.DeviceFlowAuth.Should().BeTrue();
        });

        await command.Parse("sub --type public --token test --device-flow-auth").InvokeTestAsync();
    }

    [Fact]
    public async Task InvokeAsync_FlagsArePassedWithAffiliationAndVisibility_FlagsGetParsed()
    {
        var command = new Command("sub");
        command.AddOptions(_repositoriesArguments.Options());
        command.AddOptions(_loginArguments.Options());

        command.SetAction(p =>
        {
            var repositoriesArgs = new RepositoriesArgsBinder(_repositoriesArguments, _loginArguments).Get(p);
            repositoriesArgs.Should().NotBeNull();
            repositoriesArgs.Type.Should().BeNull();
            repositoriesArgs.Affiliation.Should().Be(RepositoryAffiliation.Collaborator);
            repositoriesArgs.Visibility.Should().Be(RepositoryVisibility.Private);
            repositoriesArgs.LoginArgs.Token.Should().BeNull();
            repositoriesArgs.LoginArgs.DeviceFlowAuth.Should().BeFalse();
        });

        await command.Parse("sub --affiliation collaborator --visibility private").InvokeTestAsync();
    }
}
