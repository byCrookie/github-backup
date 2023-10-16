using FluentAssertions;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GithubBackup.TestUtils.Tests;

public class LoggerExtensionsTests
{
    [Fact]
    public void VerifyLogs_WhenHasLogs_MatchPatterns()
    {
        var logger = Substitute.For<ILogger<LoggerExtensionsTests>>();

        logger.LogDebug("1 {Time}", DateTime.Now);
        logger.LogCritical("2 {Time}", DateTime.Now);
        logger.LogError("3");

        logger.VerifyLogs(
            (LogLevel.Debug, "1 (.*)"),
            (LogLevel.Critical, "2 (.*)"),
            (LogLevel.Error, "3")
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
            (LogLevel.Debug, "1 (.*)"),
            (LogLevel.Critical, "2 (.*)"),
            (LogLevel.Error, "3"),
            (LogLevel.Information, "4")
        );

        action.Should().Throw<Exception>().WithMessage(
            """
            Valid: 1 10/12/2020 00:00:00
            Valid: 2 10/12/2021 00:00:00
            Valid: 3
            Invalid: Expected log does not exist - Expected: Information 4 - Actual:  
            
            Received:
            Debug - 1 10/12/2020 00:00:00
            Critical - 2 10/12/2021 00:00:00
            Error - 3
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
            (LogLevel.Debug, "1 (.*)"),
            (LogLevel.Critical, "2 (.*)")
        );

        action.Should().Throw<Exception>().WithMessage(
            """
            Valid: 1 10/12/2020 00:00:00
            Valid: 2 10/12/2021 00:00:00
            Invalid: Log is not expected - Expected:   - Actual: Error 3
            
            Received:
            Debug - 1 10/12/2020 00:00:00
            Critical - 2 10/12/2021 00:00:00
            Error - 3
            """
        );
    }
}