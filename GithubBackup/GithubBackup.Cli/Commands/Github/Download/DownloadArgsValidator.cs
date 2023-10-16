using FluentValidation;

namespace GithubBackup.Cli.Commands.Github.Download;

internal class DownloadArgsValidator : AbstractValidator<DownloadArgs>
{
    public DownloadArgsValidator()
    {
        RuleFor(e => e.Migrations).NotEmpty();
    }
}