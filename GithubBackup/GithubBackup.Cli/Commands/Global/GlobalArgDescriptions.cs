namespace GithubBackup.Cli.Commands.Global;

internal static class GlobalArgDescriptions
{
    public static readonly Description Quiet = new("Quiet", "Quiet", "Do not print logs to console.");
    public static readonly Description LogFile = new("LogFile", "Log file", "Path to the log file.");
    public static readonly Description Verbosity = new("Verbosity", "Verbosity", "Set the verbosity level.");
}