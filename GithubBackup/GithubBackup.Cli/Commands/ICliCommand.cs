namespace GithubBackup.Cli.Commands;

internal interface ICliCommand
{
    Task RunAsync(CancellationToken ct);
}