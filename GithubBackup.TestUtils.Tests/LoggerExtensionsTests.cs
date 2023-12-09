using System.Globalization;
using FluentAssertions;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GithubBackup.TestUtils.Tests;

public class LoggerExtensionsTests
{
    public LoggerExtensionsTests()
    {
        CultureInfo.CurrentCulture = new CultureInfo("de-CH");
    }
    
    [Fact]
    public void VerifyLogs_WhenHasLogs_MatchPatterns()
    {
        var logger = Substitute.For<ILogger<LoggerExtensionsTests>>();

        logger.LogDebug("1 {Time}", DateTime.Now);
        logger.LogCritical("2 {Time}", DateTime.Now);
        logger.LogError("3");

        logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "1 (.*)"),
            new LogEntry(LogLevel.Critical, "2 (.*)"),
            new LogEntry(LogLevel.Error, "3")
        );
    }

    [Fact]
    public void VerifyLogs_WhenHasMoreExpectedLogs_ThenThrowException()
    {
        var logger = Substitute.For<ILogger<LoggerExtensionsTests>>();

        logger.LogDebug("1 {Time}", new DateTime(2020, 10, 12));
        logger.LogCritical("2 {Time}", new DateTime(2021, 10, 12));
        logger.LogError("3");

        var action = () => logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "1 (.*)"),
            new LogEntry(LogLevel.Critical, "2 (.*)"),
            new LogEntry(LogLevel.Error, "3"),
            new LogEntry(LogLevel.Information, "4")
        );

        action.Should().Throw<Exception>().WithMessage(
            """
            Valid: 1 10/12/2020 00:00:00
            Valid: 2 10/12/2021 00:00:00
            Valid: 3
            Invalid: Expected log does not exist - Expected: Information 4 - Actual:  
            
            Received:
            new LogEntry(LogLevel.Debug, "1 10/12/2020 00:00:00"),
            new LogEntry(LogLevel.Critical, "2 10/12/2021 00:00:00"),
            new LogEntry(LogLevel.Error, "3")
            """
        );
    }
    
    [Fact]
    public void VerifyLogs_WhenHasLessExpectedLogs_ThenThrowException()
    {
        var logger = Substitute.For<ILogger<LoggerExtensionsTests>>();

        logger.LogDebug("1 {Time}", new DateTime(2020, 10, 12));
        logger.LogCritical("2 {Time}", new DateTime(2021, 10, 12));
        logger.LogError("3");

        var action = () => logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "1 (.*)"),
            new LogEntry(LogLevel.Critical, "2 (.*)")
        );

        action.Should().Throw<Exception>().WithMessage(
            """
            Valid: 1 10/12/2020 00:00:00
            Valid: 2 10/12/2021 00:00:00
            Invalid: Log is not expected - Expected:   - Actual: Error 3
            
            Received:
            new LogEntry(LogLevel.Debug, "1 10/12/2020 00:00:00"),
            new LogEntry(LogLevel.Critical, "2 10/12/2021 00:00:00"),
            new LogEntry(LogLevel.Error, "3")
            """
        );
    }
}