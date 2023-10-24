using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace GithubBackup.Cli.Tests.Utils;

internal static class TestCommandline
{
    public static Parser Build(RootCommand rootCommand)
    {
        return new CommandLineBuilder(rootCommand)
            .UseExceptionHandler((e, _) => throw new Exception(e.Message, e))
            .Build();
    }
}