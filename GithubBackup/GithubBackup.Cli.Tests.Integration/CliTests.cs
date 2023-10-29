using System.Runtime.CompilerServices;
using FluentAssertions;

namespace GithubBackup.Cli.Tests.Integration;

[UsesVerify]
public class CliTests
{
    [ModuleInitializer]
    internal static void Initialize() => VerifierSettings
        .AddScrubber(sb => sb.Replace("ReSharperTestRunner", "ghb"));
    
    [Theory]
    [InlineData("")]
    [InlineData("--help")]
    [InlineData("--version")]
    public async Task RunAsync__(string args)
    {
        var textWriter = new StringWriter();
        Console.SetOut(textWriter);

        var exitCode = await Cli.RunAsync(args.Split(" "));
            
        exitCode.Should().Be(0);
        
        await Verify(textWriter.ToString()).UseParameters(args);
    }
}