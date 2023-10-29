using System.CommandLine.IO;
using System.Runtime.CompilerServices;
using FluentAssertions;

namespace GithubBackup.Cli.Tests.Integration;

public static class TestCli
{
    public static async Task RunAsync(string args, [CallerFilePath] string sourceFile = "")
    {
        var testConsole = new TestConsole();

        var exitCode = await Cli.RunAsync(args.Split(" "), testConsole);

        exitCode.Should().Be(0);
        
        var settings = new VerifySettings();
        settings.AddScrubber(sb => sb.Replace("ReSharperTestRunner", "ghb"));

        // ReSharper disable once ExplicitCallerInfoArgument
        await Verify(testConsole.Out.ToString(), settings, sourceFile).UseParameters(args);
    }
}