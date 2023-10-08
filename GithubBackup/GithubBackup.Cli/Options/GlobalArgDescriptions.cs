using GithubBackup.Cli.Commands;

namespace GithubBackup.Cli.Options;

public static class GlobalArgDescriptions
{
    public static readonly Description Quiet = new("Quiet", "Quiet", "Do not print logs to console. Can only be used when interactive mode is disabled.");
    public static readonly Description LogFile = new("LogFile", "Log file", "Path to the log file.");
    public static readonly Description Verbosity = new("Verbosity", "Verbosity", "Set the verbosity level.");
    public static readonly Description Interactive = new("Interactive", "Interactive", "Enable interactive mode. This will prompt for user input when needed.");
}