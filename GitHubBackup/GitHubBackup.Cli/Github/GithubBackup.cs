using GitHubBackup.Cli.Options;

namespace GitHubBackup.Cli.Github;

internal class GithubBackup : IGithubBackup
{
    private readonly GlobalArgs _globalArgs;
    private readonly GithubBackupArgs _backupArgs;

    public GithubBackup(GlobalArgs globalArgs, GithubBackupArgs backupArgs)
    {
        _globalArgs = globalArgs;
        _backupArgs = backupArgs;
    }

    public Task RunAsync()
    {
        if (_globalArgs.Interactive)
        {
            Console.WriteLine($"Do you want to backup to {_backupArgs.Destination}? (y/n)");
            if (!new List<ConsoleKey> { ConsoleKey.Y }.Contains(Console.ReadKey().Key))
            {
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}