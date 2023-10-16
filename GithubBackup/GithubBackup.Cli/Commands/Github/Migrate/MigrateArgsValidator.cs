using FluentValidation;
using GithubBackup.Cli.Commands.Github.Download;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal class MigrateArgsValidator : AbstractValidator<MigrateArgs>
{
    public MigrateArgsValidator()
    {
        RuleFor(e => e.Repositories).NotEmpty();
    }
}