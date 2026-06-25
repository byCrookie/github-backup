using GithubBackup.Cli.Commands.Global;

namespace GithubBackup.Cli.Output;

internal sealed class CliOutput(GlobalArgs globalArgs, CliOutputOptions options) : ICliOutput
{
    public void Data(string message)
    {
        options.Output.WriteLine(message);
    }

    public void Status(string message)
    {
        if (!globalArgs.Quiet)
        {
            options.Error.WriteLine(message);
        }
    }

    public void Error(string message)
    {
        options.Error.WriteLine(message);
    }
}
