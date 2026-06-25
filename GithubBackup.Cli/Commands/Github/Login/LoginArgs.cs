namespace GithubBackup.Cli.Commands.Github.Login;

internal sealed record LoginArgs(string? Token, bool DeviceFlowAuth);
