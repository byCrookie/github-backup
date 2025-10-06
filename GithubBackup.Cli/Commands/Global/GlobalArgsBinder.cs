using System.CommandLine;

namespace GithubBackup.Cli.Commands.Global;

internal sealed class GlobalArgsBinder
{
    private readonly GlobalArguments _globalArguments;

    public GlobalArgsBinder(GlobalArguments globalArguments)
    {
        _globalArguments = globalArguments;
    }

    public GlobalArgs Get(ParseResult parseResult)
    {
        var verbosity = parseResult.GetRequiredValue(
            _globalArguments.VerbosityOption
        );
        var quiet = parseResult.GetRequiredValue(
            _globalArguments.QuietOption
        );
        var logFile = parseResult.GetValue(_globalArguments.LogFileOption);
        return new GlobalArgs(verbosity, quiet, logFile);
    }
}
