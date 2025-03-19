using FluentValidation;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal class MigrateArgsValidator : AbstractValidator<MigrateArgs>
{
    public MigrateArgsValidator()
    {
        RuleFor(e => e.Repositories).NotEmpty().When(e => !e.OrgMetadataOnly);
        RuleFor(e => e.Repositories)
            .Empty()
            .When(e => e.OrgMetadataOnly)
            .WithMessage(OrgMetadataOnlyMustBeUsedAlone);
    }

    public static string OrgMetadataOnlyMustBeUsedAlone =>
        "Cannot specify repositories when only migrating org metadata only.";
}
