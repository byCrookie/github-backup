using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Global;

internal sealed record GlobalArgs(LogLevel Verbosity, bool Quiet, FileInfo? LogFile);
