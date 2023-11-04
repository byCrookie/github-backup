using FluentValidation;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal class MigrationsArgsValidator : AbstractValidator<MigrationsArgs>
{
    public MigrationsArgsValidator()
    {
        RuleFor(e => e.Since).Empty().When(e => e.DaysOld is not null).WithMessage(CannotSpecifySinceAndDaysOld);
    }
    
    public static string CannotSpecifySinceAndDaysOld => "Cannot specify both --days-old and --since.";
}