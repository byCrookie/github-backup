namespace GithubBackup.Cli.Commands;

internal interface ICommandRunner
{
    Task RunAsync(CancellationToken ct);
}