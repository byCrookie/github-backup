using System.Diagnostics;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GithubBackup.TestUtils.Logging;

public static class LoggerExtensions
{
    public static void VerifyLogs<T>(this ILogger<T> logger, params (LogLevel Level, string? Pattern)[] expectedLogs)
    {
        logger.Should().NotBeNull();
        var logs = logger
            .ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(logger.Log))
            .Select(call => new LogEntry(
                    (LogLevel?)call.GetArguments()[0] ?? throw new Exception("loglevel can not be null"),
                    call.GetArguments()[2]?.ToString()
                )
            ).ToList();
        var expectedLogEntries = expectedLogs
            .Select(expectedLog => new LogEntry(expectedLog.Level, expectedLog.Pattern))
            .ToList();

        var logVerifications = new List<LogVerification>();

        for (var i = 0; i < Math.Max(logs.Count, expectedLogEntries.Count); i++)
        {
            var expectedLog = expectedLogEntries.Count > i ? expectedLogEntries[i] : null;
            var log = logs.Count > i ? logs[i] : null;

            if (expectedLog is null && log is not null)
            {
                logVerifications.Add(new LogVerification(null, null, log.Level, log.Message, false, "Log is not expected"));
                continue;
            }

            if (log is null && expectedLog is not null)
            {
                logVerifications.Add(new LogVerification(expectedLog.Level, expectedLog.Message, null, null, false, "Expected log does not exist"));
                continue;
            }

            if (expectedLog is null && log is null)
            {
                throw new UnreachableException();
            }

            if (expectedLog!.Level != log!.Level)
            {
                logVerifications.Add(new LogVerification(expectedLog.Level, expectedLog.Message, log.Level, log.Message, false, "Log level does not match"));
                continue;
            }

            var regex = new Regex(expectedLog.Message ?? string.Empty);
            if (!regex.IsMatch(log.Message ?? string.Empty))
            {
                logVerifications.Add(new LogVerification(expectedLog.Level, expectedLog.Message, log.Level, log.Message, false, "Pattern does not match"));
                continue;
            }

            logVerifications.Add(new LogVerification(expectedLog.Level, expectedLog.Message, log.Level, log.Message, true));
        }

        if (logVerifications.All(logVerification => logVerification.Valid))
        {
            return;
        }

        var errorMessage = string.Join(Environment.NewLine, logVerifications);
        throw new Exception(errorMessage);
    }
}