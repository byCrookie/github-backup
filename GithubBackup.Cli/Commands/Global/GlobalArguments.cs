using System.CommandLine;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Global;

public class GlobalArguments
{
    public Option<bool> QuietOption { get; } =
        new(
            name: "--quiet",
            aliases: ["-q"]
        )
        {
            Description = GlobalArgDescriptions.Quiet.Long,
            DefaultValueFactory = _ => true,
            Recursive = true
        };

    public Option<FileInfo?> LogFileOption { get; } =
        new(
            name: "--log-file",
            aliases: ["-l"]
        )
        {
            Description = GlobalArgDescriptions.LogFile.Long,
            Recursive = true
        };

    public Option<LogLevel> VerbosityOption { get; } =
        new(
            name: "--verbosity",
            aliases: ["-v"]
        )
        {
            Description = GlobalArgDescriptions.Verbosity.Long,
            DefaultValueFactory = _ => LogLevel.Information,
            Recursive = true
        };

    public IEnumerable<Option> Options()
    {
        return [QuietOption, LogFileOption, VerbosityOption];
    }
}
