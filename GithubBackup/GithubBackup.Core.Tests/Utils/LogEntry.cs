using Microsoft.Extensions.Logging;

namespace GithubBackup.Core.Tests.Utils;

public record LogEntry(LogLevel Level, string? Message);