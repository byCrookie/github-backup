namespace GithubBackup.Cli.Tests.Integration;

[UsesVerify]
public class CliTests
{
    [Theory]
    [InlineData("")]
    [InlineData("--help")]
    [InlineData("--version")]
    public async Task RunAsync__(string args)
    {
        await TestCli.RunAsync(args);
    }
}