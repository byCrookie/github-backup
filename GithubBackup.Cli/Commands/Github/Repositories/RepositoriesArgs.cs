using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Core.Github.Repositories;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal sealed record RepositoriesArgs(
    RepositoryType? Type,
    RepositoryAffiliation? Affiliation,
    RepositoryVisibility? Visibility,
    LoginArgs LoginArgs
);
