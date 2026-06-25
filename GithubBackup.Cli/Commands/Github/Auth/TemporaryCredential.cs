namespace GithubBackup.Cli.Commands.Github.Auth;

internal sealed record TemporaryCredential(string Token, DateTimeOffset? ExpiresAt);
