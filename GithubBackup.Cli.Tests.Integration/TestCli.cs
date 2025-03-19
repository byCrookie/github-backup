using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Flurl.Http.Testing;
using GithubBackup.Cli.Boot;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Cli.Tests.Integration;

public static class TestCli
{
    public static async Task RunAsync(
        string args,
        int expectedExitCode,
        Action<HttpTest>? configureHttp = null,
        Action<MockFileSystem>? configureFileSystem = null,
        IDictionary<string, string>? environmentVariables = null,
        [CallerFilePath] string sourceFile = ""
    )
    {
        var mockFileSystem = new MockFileSystem();
        configureFileSystem?.Invoke(mockFileSystem);

        foreach (
            var environmentVariable in environmentVariables ?? new Dictionary<string, string>()
        )
        {
            Environment.SetEnvironmentVariable(environmentVariable.Key, environmentVariable.Value);
        }

        var testConsole = new TestConsole(new Spectre.Console.Testing.TestConsole());

        using var httpTest = new HttpTest();
        configureHttp?.Invoke(httpTest);

        var exitCode = await Boot.Cli.RunAsync(
            args.Split(" "),
            new CliOptions
            {
                Console = testConsole,
                AfterServices = hb => hb.Services.AddSingleton<IFileSystem>(mockFileSystem),
            }
        );

        var settings = new VerifySettings();
        settings.AddScrubber(sb => sb.Replace("ReSharperTestRunner", "ghb"));
        settings.AddScrubber(sb => sb.Replace("testhost", "ghb"));

        // ReSharper disable once ExplicitCallerInfoArgument
        await Verify(testConsole.Out.ToString(), settings, sourceFile).UseParameters(args);

        exitCode.Should().Be(expectedExitCode);
    }
}
