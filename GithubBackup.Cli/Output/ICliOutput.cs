namespace GithubBackup.Cli.Output;

internal interface ICliOutput
{
    void Data(string message);

    void Status(string message);

    void Error(string message);
}
