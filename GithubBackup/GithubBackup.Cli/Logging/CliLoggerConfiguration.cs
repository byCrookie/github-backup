using GithubBackup.Cli.Options;
using GithubBackup.Cli.Utils;
using Serilog;

namespace GithubBackup.Cli.Logging;

internal static class CliLoggerConfiguration
{
    public static LoggerConfiguration Create(GlobalArgs globalArgs)
    {
        var logEventLevel = globalArgs.Verbosity.MicrosoftToSerilogLevel();
        
        var configuration = new LoggerConfiguration();

        if (globalArgs.LogFile is not null)
        {
            configuration.WriteTo.File(
                globalArgs.LogFile.FullName,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 100_000_000,
                retainedFileCountLimit: 10,
                rollingInterval: RollingInterval.Infinite
            ).MinimumLevel.Is(logEventLevel);
        }
        
        return configuration;
    }
}