using System.CommandLine;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Options;

internal class GlobalArgs
{
    public LogLevel Verbosity { get; }
    public bool Quiet { get; }
    public FileInfo? LogFile { get; }
    public bool Interactive { get; }

    public GlobalArgs(LogLevel verbosity, bool quiet, FileInfo? logFile, bool interactive)
    {
        Verbosity = verbosity;
        Quiet = quiet;
        LogFile = logFile;
        Interactive = interactive;
    }

    public static Option<bool> QuietOption { get; } = new(
        aliases: new[] { "-q", "--quiet" },
        getDefaultValue: () => false,
        description: "Do not print logs to console"
    );

    public static Option<FileInfo?> LogFileOption { get; } = new(
        aliases: new[] { "-l", "--log-file" },
        description: "The path to the log file"
    );

    public static Option<LogLevel> VerbosityOption { get; } = new(
        aliases: new[] { "-v", "--verbosity" },
        getDefaultValue: () => LogLevel.Warning,
        description: "The verbosity of the logs"
    );

    public static Option<bool> InteractiveOption { get; } = new(
        aliases: new[] { "-i", "--interactive" },
        getDefaultValue: () => false,
        description: "Select backup customizations interactively"
    );
}