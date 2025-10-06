using System.CommandLine;

namespace GithubBackup.Cli.Commands.Github.Manual;

internal sealed class ManualBackupArgsBinder
{
    public ManualBackupArgs Get(ParseResult parseResult)
    {
        return new ManualBackupArgs();
    }
}
