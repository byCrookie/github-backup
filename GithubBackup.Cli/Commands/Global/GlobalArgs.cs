using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Global;

internal sealed class GlobalArgs
{
    public LogLevel Verbosity { get; }
    public bool Quiet { get; }
    public FileInfo? LogFile { get; }

    public GlobalArgs(LogLevel verbosity, bool quiet, FileInfo? logFile)
    {
        Verbosity = verbosity;
        Quiet = quiet;
        LogFile = logFile;
    }
}
