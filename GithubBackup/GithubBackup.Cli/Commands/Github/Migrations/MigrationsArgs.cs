using FluentValidation;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class MigrationsArgs
{
    public bool Export { get; }
    public long? DaysOld { get; }
    public DateTime? Since { get; }

    public MigrationsArgs(bool export, long? daysOld, DateTime? since)
    {
        Export = export;
        DaysOld = daysOld;
        Since = since;
        
        new MigrationsArgsValidator().ValidateAndThrow(this);
    }
}