using System.CommandLine;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Global;

public class GlobalArguments
{
    public Option<bool> QuietOption { get; } = new(
        aliases: new[] { "-q", "--quiet" },
        getDefaultValue: () => false,
        description: GlobalArgDescriptions.Quiet.Long
    );

    public Option<FileInfo?> LogFileOption { get; } = new(
        aliases: new[] { "-l", "--log-file" },
        description: GlobalArgDescriptions.LogFile.Long
    );

    public Option<LogLevel> VerbosityOption { get; } = new(
        aliases: new[] { "-v", "--verbosity" },
        getDefaultValue: () => LogLevel.Information,
        description: GlobalArgDescriptions.Verbosity.Long
    );

    public IEnumerable<Option> Options()
    {
        return new Option[]
        {
            QuietOption,
            LogFileOption,
            VerbosityOption
        };
    }
}