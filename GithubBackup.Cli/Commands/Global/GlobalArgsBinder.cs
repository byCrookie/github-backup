using System.CommandLine;

namespace GithubBackup.Cli.Commands.Global;

internal sealed class GlobalArgsBinder(GlobalArguments globalArguments)
{
    public GlobalArgs Get(ParseResult parseResult)
    {
        var verbosity = parseResult.GetRequiredValue(globalArguments.VerbosityOption);
        var quiet = parseResult.GetRequiredValue(globalArguments.QuietOption);
        var logFile = parseResult.GetValue(globalArguments.LogFileOption);
        return new GlobalArgs(verbosity, quiet, logFile);
    }
}
