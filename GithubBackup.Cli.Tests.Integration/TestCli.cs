using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Flurl.Http.Testing;
using GithubBackup.Cli.Boot;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Testing;

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

        var testConsole = new TestConsole();

        using var httpTest = new HttpTest();
        configureHttp?.Invoke(httpTest);

        var exitCode = await Boot.Cli.RunAsync(
            args.Split(" "),
            new CliOptions
            {
                EnableDefaultExceptionHandler = false,
                Output = testConsole.Profile.Out.Writer,
                Error = testConsole.Profile.Out.Writer,
                AfterServices = hb => hb.Services.AddSingleton<IFileSystem>(mockFileSystem),
            }
        );

        var settings = new VerifySettings();
        settings.AddScrubber(sb => sb.Replace("ReSharperTestRunner", "ghb"));
        settings.AddScrubber(sb => sb.Replace("testhost", "ghb"));
        settings.AddScrubber(sb =>
        {
            var scrubbed = Regex.Replace(
                sb.ToString(),
                @"Command finished\. Duration: .*",
                "Command finished. Duration: <duration>"
            );
            sb.Clear();
            sb.Append(scrubbed);
        });

        await Verify(testConsole.Output.TrimEnd(), settings, sourceFile)
            .UseParameters(args)
            .ScrubLinesWithReplace(s =>
                s.Replace(
                    Assembly.GetExecutingAssembly().GetName().Name
                        ?? throw new InvalidOperationException(
                            "Cannot get executing assembly name"
                        ),
                    "ghb"
                )
            );

        exitCode.Should().Be(expectedExitCode);
    }
}
