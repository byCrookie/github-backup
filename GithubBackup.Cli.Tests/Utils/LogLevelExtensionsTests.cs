using AwesomeAssertions;
using GithubBackup.Cli.Utils;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace GithubBackup.Cli.Tests.Utils;

public class LogLevelExtensionsTests
{
    [Theory]
    [InlineData(LogLevel.None, LogEventLevel.Fatal)]
    [InlineData(LogLevel.Critical, LogEventLevel.Fatal)]
    [InlineData(LogLevel.Error, LogEventLevel.Error)]
    [InlineData(LogLevel.Warning, LogEventLevel.Warning)]
    [InlineData(LogLevel.Information, LogEventLevel.Information)]
    [InlineData(LogLevel.Debug, LogEventLevel.Debug)]
    [InlineData(LogLevel.Trace, LogEventLevel.Verbose)]
    public void MicrosoftToSerilogLevel_LogLevel_LogEventLevel(
        LogLevel logLevel,
        LogEventLevel expected
    )
    {
        var result = logLevel.MicrosoftToSerilogLevel();
        result.Should().Be(expected);
    }
}
