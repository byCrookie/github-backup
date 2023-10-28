using FluentValidation;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Commands.Services;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal sealed class MigrateArgs : ICommandIntervalArgs
{
    public string[] Repositories { get; }
    public bool LockRepositories { get; }
    public bool ExcludeMetadata { get; }
    public bool ExcludeGitData { get; }
    public bool ExcludeAttachements { get; }
    public bool ExcludeReleases { get; }
    public bool ExcludeOwnerProjects { get; }
    public bool OrgMetadataOnly { get; }
    public IntervalArgs IntervalArgs { get; }
    public LoginArgs LoginArgs { get; }

    public MigrateArgs(
        string[] repositories,
        bool lockRepositories,
        bool excludeMetadata,
        bool excludeGitData,
        bool excludeAttachements,
        bool excludeReleases,
        bool excludeOwnerProjects,
        bool orgMetadataOnly,
        IntervalArgs intervalArgs,
        LoginArgs loginArgs
    )
    {
        Repositories = repositories;
        LockRepositories = lockRepositories;
        ExcludeMetadata = excludeMetadata;
        ExcludeGitData = excludeGitData;
        ExcludeAttachements = excludeAttachements;
        ExcludeReleases = excludeReleases;
        ExcludeOwnerProjects = excludeOwnerProjects;
        OrgMetadataOnly = orgMetadataOnly;
        IntervalArgs = intervalArgs;
        LoginArgs = loginArgs;

        new MigrateArgsValidator().ValidateAndThrow(this);
    }
}