using ByteSizeLib;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;
using Serilog;

namespace GithubBackup.Cli.Logging;

internal static class CliLogger
{
    public static LoggerConfiguration Create(GlobalArgs globalArgs)
    {
        var logEventLevel = globalArgs.Verbosity.MicrosoftToSerilogLevel();

        var configuration = new LoggerConfiguration();

        if (globalArgs.LogFile is not null)
        {
            configuration
                .WriteTo.File(
                    globalArgs.LogFile.FullName,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: (long)ByteSize.FromMegaBytes(50).Bytes,
                    retainedFileCountLimit: 2
                )
                .MinimumLevel.Is(logEventLevel);
        }

        return configuration;
    }
}
