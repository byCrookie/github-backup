namespace GithubBackup.Cli.Tests.Integration.Commands.Github.Login;

[UsesVerify]
public class LoginTests
{
    [Theory]
    [InlineData("login --help")]
    [InlineData("login", 1)]
    public async Task RunAsync__(string args, int exitCode = 0)
    {
        await TestCli.RunAsync(args, exitCode, _ => {});
    }
    
    [Fact]
    public async Task RunAsync_LoginUsingToken_ThrowsException()
    {
        var args = "login";
        
        await TestCli.RunAsync(args, 1, _ => {});
    }
}