using System.Runtime.CompilerServices;
using FluentAssertions;
using Flurl.Http.Testing;

namespace GithubBackup.Cli.Tests.Integration;

public static class TestCli
{
    public static async Task RunAsync(string args, int expectedExitCode, Action<HttpTest> configureHttp, [CallerFilePath] string sourceFile = "")
    {
        var testConsole = new TestConsole(new Spectre.Console.Testing.TestConsole());

        using var httpTest = new HttpTest();
        configureHttp(httpTest);

        var exitCode = await Cli.RunAsync(args.Split(" "), testConsole);

        var settings = new VerifySettings();
        settings.AddScrubber(sb => sb.Replace("ReSharperTestRunner", "ghb"));

        // ReSharper disable once ExplicitCallerInfoArgument
        await Verify(testConsole.Out.ToString(), settings, sourceFile).UseParameters(args);
        
        exitCode.Should().Be(expectedExitCode);
    }
}