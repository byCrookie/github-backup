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
        Login to Github. Tokens are resolved from --token, GITHUB_BACKUP_TOKEN, or GitHub device flow.
        Device flow tokens are cached temporarily while valid. {0}
        """;

    public static Command Create(string[] args, CommandOptions options)
    {
        var homeDirectoryDescription = GetHomeDirectoryDescription();
        var description = string.Format(CommandDescription, homeDirectoryDescription);

        var command = new Command(CommandName, description);
        var loginArguments = new LoginArguments();
        command.AddOptions(loginArguments.Options());

        command.SetAction(
            (r, ct) =>
            {
                var globalArgs = new GlobalArgsBinder(options.GlobalArguments).Get(r);
                var loginArgs = new LoginArgsBinder(loginArguments).Get(r);

                var runner = new CliRunner<LoginRunner, LoginArgs>(
                    args,
                    globalArgs,
                    loginArgs,
                    new RunOptions
                    {
                        Output = options.Output,
                        Error = options.Error,
                        AfterServices = options.AfterServices,
                    }
                );

                return runner.RunAsync(ct);
            }
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
