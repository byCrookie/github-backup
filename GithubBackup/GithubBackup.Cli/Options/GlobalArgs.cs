using System.CommandLine;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GithubBackup.Cli.Options;

public class GlobalArgs
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

    public static Option<bool> QuietOption { get; }

    public static Option<FileInfo?> LogFileOption { get; }

    public static Option<LogLevel> VerbosityOption { get; }

    public static Option<bool> InteractiveOption { get; }

    static GlobalArgs()
    {
        QuietOption = new Option<bool>(
            aliases: new[] { "-q", "--quiet" },
            getDefaultValue: () => false,
            description: "Do not print logs to console. Can only be used when interactive mode is disabled."
        );

        LogFileOption = new Option<FileInfo?>(
            aliases: new[] { "-l", "--log-file" },
            description: "The path to the log file"
        );

        VerbosityOption = new Option<LogLevel>(
            aliases: new[] { "-v", "--verbosity" },
            getDefaultValue: () => LogLevel.Warning,
            description: "The verbosity of the logs"
        );

        InteractiveOption = new Option<bool>(
            aliases: new[] { "-i", "--interactive" },
            getDefaultValue: () => false,
            description: "Enable interactive mode. This will prompt for user input when needed."
        );

        QuietOption.AddValidator(result =>
        {
            var interactive = result.GetValueForOption(InteractiveOption);
            var quiet = result.GetValueOrDefault<bool>();

            if (interactive && quiet)
            {
                result.ErrorMessage = "Quiet mode can only be used when interactive mode is disabled.";
            }
        });
    }
}