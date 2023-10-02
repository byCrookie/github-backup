namespace GithubBackup.Cli.Commands;

public interface ICliCommand
{
    Task RunAsync(CancellationToken ct);
}