namespace GithubBackup.Cli.Tests.Integration;


public class CliTests
{
    [Theory]
    [InlineData("", 1)]
    [InlineData("--help")]
    public async Task RunAsync__(string args, int exitCode = 0)
    {
        await TestCli.RunAsync(args, exitCode, _ => {});
    }
}