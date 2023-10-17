using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace GithubBackup.TestUtils.Logging;

public record LogEntry(LogLevel Level, [StringSyntax(StringSyntaxAttribute.Regex)] string? Message)
{
    public override string ToString()
    {
        return $"{Level} - {Message}";
    }
};