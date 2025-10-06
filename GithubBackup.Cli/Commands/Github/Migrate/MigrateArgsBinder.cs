using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Interval;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal sealed class MigrateArgsBinder(
    MigrateArguments migrateArguments,
    IntervalArguments intervalArguments,
    LoginArguments loginArguments)
{
    public MigrateArgs Get(ParseResult parseResult)
    {
        var repositories = parseResult.GetRequiredValue(
            migrateArguments.RepositoriesOption
        );
        var lockRepositories = parseResult.GetRequiredValue(
            migrateArguments.LockRepositoriesOption
        );
        var excludeMetadata = parseResult.GetRequiredValue(
            migrateArguments.ExcludeMetadataOption
        );
        var excludeGitData = parseResult.GetRequiredValue(
            migrateArguments.ExcludeGitDataOption
        );
        var excludeAttachements = parseResult.GetRequiredValue(
            migrateArguments.ExcludeAttachementsOption
        );
        var excludeReleases = parseResult.GetRequiredValue(
            migrateArguments.ExcludeReleasesOption
        );
        var excludeOwnerProjects = parseResult.GetRequiredValue(
            migrateArguments.ExcludeOwnerProjectsOption
        );
        var excludeMetadataOnly = parseResult.GetRequiredValue(
            migrateArguments.OrgMetadataOnlyOption
        );

        var intervalArgs = new IntervalArgsBinder(intervalArguments).Get(parseResult);
        var loginArgs = new LoginArgsBinder(loginArguments).Get(parseResult);

        return new MigrateArgs(
            repositories,
            lockRepositories,
            excludeMetadata,
            excludeGitData,
            excludeAttachements,
            excludeReleases,
            excludeOwnerProjects,
            excludeMetadataOnly,
            intervalArgs,
            loginArgs
        );
    }
}
