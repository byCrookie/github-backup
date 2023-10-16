using Microsoft.Extensions.Logging;

namespace GithubBackup.TestUtils.Logging;

public record LogEntry(LogLevel Level, string? Message)
{
    public override string ToString()
    {
        return $"{Level} - {Message}";
    }
};