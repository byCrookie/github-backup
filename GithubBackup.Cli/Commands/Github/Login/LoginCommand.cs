using System.CommandLine;
using GithubBackup.Cli.Boot;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Login;

internal static class LoginCommand
{
    private const string CommandName = "login";

    private const string CommandDescription = """
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
            (globalArgs, loginArgs) => RunAsync(args, globalArgs, loginArgs, options),
            new GlobalArgsBinder(options.GlobalArguments),
            new LoginArgsBinder(loginArguments)
        );

        return command;
    }

    private static Task RunAsync(
        string[] args,
        GlobalArgs globalArgs,
        LoginArgs loginArgs,
        CommandOptions options
    )
    {
        var runner = new CliRunner<LoginRunner, LoginArgs>(
            args,
            globalArgs,
            loginArgs,
            new RunOptions { AfterServices = options.AfterServices }
        );

        return runner.RunAsync();
    }

    private static string GetHomeDirectoryDescription()
    {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return string.IsNullOrWhiteSpace(homeDirectory)
            ? $"{Environment.NewLine}ERROR: Could not determine home directory. Login command will not work."
            : string.Empty;
    }
}
