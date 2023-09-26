using GitHubBackup.Cli.Utils;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GitHubBackup.Cli.Logging;

internal static class CliLoggerConfiguration
{
    public static LoggerConfiguration Create(LogLevel logLevel, FileSystemInfo? fileSystemInfo, bool quiet)
    {
        var configuration = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel.MicrosoftToSerilogLevel());

        if (fileSystemInfo is not null)
        {
            configuration.WriteTo.File(
                fileSystemInfo.FullName,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 100_000_000,
                retainedFileCountLimit: 10,
                rollingInterval: RollingInterval.Infinite
            );
        }

        if (!quiet)
        {
            configuration.WriteTo.Console();
        }

        return configuration;
    }
}