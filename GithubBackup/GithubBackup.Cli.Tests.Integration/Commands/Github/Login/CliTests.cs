namespace GithubBackup.Cli.Tests.Integration.Commands.Github.Login;

[UsesVerify]
public class LoginTests
{
    [Theory]
    [InlineData("login --help")]
    [InlineData("login")]
    public async Task RunAsync__(string args)
    {
        await TestCli.RunAsync(args);
    }
}