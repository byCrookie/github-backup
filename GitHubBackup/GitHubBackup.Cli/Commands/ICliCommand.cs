namespace GitHubBackup.Cli.Commands;

public interface ICliCommand
{
    Task RunAsync();
}