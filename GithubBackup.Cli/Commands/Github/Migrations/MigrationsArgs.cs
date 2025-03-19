using FluentValidation;
using GithubBackup.Cli.Commands.Github.Login;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class MigrationsArgs
{
    public bool Export { get; }
    public long? DaysOld { get; }
    public DateTime? Since { get; }
    public LoginArgs LoginArgs { get; }

    public MigrationsArgs(bool export, long? daysOld, DateTime? since, LoginArgs loginArgs)
    {
        Export = export;
        DaysOld = daysOld;
        Since = since;
        LoginArgs = loginArgs;

        new MigrationsArgsValidator().ValidateAndThrow(this);
    }
}
