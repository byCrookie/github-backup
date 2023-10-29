using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Login;

internal static class LoginCommand
{
    private const string CommandName = "login";

    private const string CommandDescription =
        """
        Login to Github. Persists your login token to disk for future use.
        Only one login token can be persisted at a time. {0}
        """;

    public static Command Create(string[] args, CommandOptions options)
    {
        var homeDirectoryDescription = GetHomeDirectoryDescription();
        var description = string.Format(CommandDescription, homeDirectoryDescription);

        var command = new Command(CommandName, description);
        var loginArguments = new LoginArguments();
        command.AddOptions(loginArguments.Options());

        command.SetHandler(
            (globalArgs, loginArgs) => GithubBackup.Cli.Cli
                .RunAsync<LoginRunner, LoginArgs>(args, globalArgs, loginArgs, new RunOptions
                {
                    AfterConfiguration = options.AfterConfiguration,
                    AfterServices = options.AfterServices
                }),
            new GlobalArgsBinder(options.GlobalArguments),
            new LoginArgsBinder(loginArguments)
        );

        return command;
    }

    private static string GetHomeDirectoryDescription()
    {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return string.IsNullOrWhiteSpace(homeDirectory)
            ? $"{Environment.NewLine}ERROR: Could not determine home directory. Login command will not work."
            : string.Empty;
    }
}