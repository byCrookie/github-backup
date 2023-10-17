using System.CommandLine;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Global;

internal sealed class GlobalArgs
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
            description: GlobalArgDescriptions.Quiet.Long
        );

        LogFileOption = new Option<FileInfo?>(
            aliases: new[] { "-l", "--log-file" },
            description: GlobalArgDescriptions.LogFile.Long
        );

        VerbosityOption = new Option<LogLevel>(
            aliases: new[] { "-v", "--verbosity" },
            getDefaultValue: () => LogLevel.Information,
            description: GlobalArgDescriptions.Verbosity.Long
        );

        InteractiveOption = new Option<bool>(
            aliases: new[] { "-i", "--interactive" },
            getDefaultValue: () => false,
            description: GlobalArgDescriptions.Interactive.Long
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

    public static Option[] Options()
    {
        return new Option[]
        {
            QuietOption,
            LogFileOption,
            VerbosityOption,
            InteractiveOption
        };
    }
}