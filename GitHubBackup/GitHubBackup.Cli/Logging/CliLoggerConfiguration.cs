using GitHubBackup.Cli.Options;
using GitHubBackup.Cli.Utils;
using Serilog;

namespace GitHubBackup.Cli.Logging;

internal static class CliLoggerConfiguration
{
    public static LoggerConfiguration Create(GlobalArgs globalArgs)
    {
        var configuration = new LoggerConfiguration()
            .MinimumLevel.Is(globalArgs.Verbosity.MicrosoftToSerilogLevel());

        if (globalArgs.LogFile is not null)
        {
            configuration.WriteTo.File(
                globalArgs.LogFile.FullName,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 100_000_000,
                retainedFileCountLimit: 10,
                rollingInterval: RollingInterval.Infinite
            );
        }

        if (!globalArgs.Quiet)
        {
            configuration.WriteTo.Console();
        }

        return configuration;
    }
}