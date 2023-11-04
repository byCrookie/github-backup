using Microsoft.Extensions.Logging;

namespace GithubBackup.TestUtils.Logging;

public record LogVerification(
    LogLevel? ExpectedLevel,
    string? ExpectedMessagePattern,
    LogLevel? Level,
    string? Message,
    bool Valid,
    string? ErrorMessage = null
)
{
    public override string ToString()
    {
        return Valid ? $"Valid: {Message}" : $"Invalid: {ErrorMessage} - Expected: {ExpectedLevel} {ExpectedMessagePattern} - Actual: {Level} {Message}";
    }
};