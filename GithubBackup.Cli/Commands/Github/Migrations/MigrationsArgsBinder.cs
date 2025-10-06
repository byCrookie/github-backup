using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Login;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class MigrationsArgsBinder(
    MigrationsArguments migrationsArguments,
    LoginArguments loginArguments)
{
    public MigrationsArgs Get(ParseResult parseResult)
    {
        var export = parseResult.GetRequiredValue(
            migrationsArguments.ExportOption
        );
        var since = parseResult.GetValue(migrationsArguments.SinceOption);
        var daysOld = parseResult.GetValue(
            migrationsArguments.DaysOldOption
        );

        var loginArgs = new LoginArgsBinder(loginArguments).Get(parseResult);

        return new MigrationsArgs(export, daysOld, since, loginArgs);
    }
}
