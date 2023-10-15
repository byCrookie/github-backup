using Microsoft.Extensions.Logging;

namespace GithubBackup.TestUtils.Logging;

public record LogEntry(LogLevel Level, string? Message);