namespace GithubBackup.Cli.Tests.Integration.Commands.Github.Login;

[UsesVerify]
public class LoginTests
{
    [Theory]
    [InlineData("login --help")]
    [InlineData("login", 1)]
    [InlineData("login --token test_token", 1)]
    public async Task RunAsync__(string args, int exitCode = 0)
    {
        await TestCli.RunAsync(args, exitCode, _ => {});
    }
}