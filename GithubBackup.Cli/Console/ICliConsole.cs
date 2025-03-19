using System.CommandLine;

namespace GithubBackup.Cli.Console;

public interface ICliConsole : IConsole
{
    public void WriteException(Exception exception);
}
