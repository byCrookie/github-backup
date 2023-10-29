namespace GithubBackup.Cli.Tests.Integration;

[UsesVerify]
public class CliTests
{
    [Theory]
    [InlineData("", 1)]
    [InlineData("--help")]
    [InlineData("--version")]
    public async Task RunAsync__(string args, int exitCode = 0)
    {
        await TestCli.RunAsync(args, exitCode, _ => {});
    }
}