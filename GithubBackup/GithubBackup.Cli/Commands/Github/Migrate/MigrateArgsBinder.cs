using System.CommandLine.Binding;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal sealed class MigrateArgsBinder : BinderBase<MigrateArgs>
{
    private readonly MigrateArguments _migrateArguments;
    private readonly IntervalArguments _intervalArguments;

    public MigrateArgsBinder(MigrateArguments migrateArguments, IntervalArguments intervalArguments)
    {
        _migrateArguments = migrateArguments;
        _intervalArguments = intervalArguments;
    }
    
    public MigrateArgs Get(BindingContext bindingContext) => GetBoundValue(bindingContext);
    
    protected override MigrateArgs GetBoundValue(BindingContext bindingContext)
    {
        var repositories = bindingContext.ParseResult.GetRequiredValueForOption(_migrateArguments.RepositoriesOption);
        var lockRepositories = bindingContext.ParseResult.GetRequiredValueForOption(_migrateArguments.LockRepositoriesOption);
        var excludeMetadata = bindingContext.ParseResult.GetRequiredValueForOption(_migrateArguments.ExcludeMetadataOption);
        var excludeGitData = bindingContext.ParseResult.GetRequiredValueForOption(_migrateArguments.ExcludeGitDataOption);
        var excludeAttachements = bindingContext.ParseResult.GetRequiredValueForOption(_migrateArguments.ExcludeAttachementsOption);
        var excludeReleases = bindingContext.ParseResult.GetRequiredValueForOption(_migrateArguments.ExcludeReleasesOption);
        var excludeOwnerProjects = bindingContext.ParseResult.GetRequiredValueForOption(_migrateArguments.ExcludeOwnerProjectsOption);
        var excludeMetadataOnly = bindingContext.ParseResult.GetRequiredValueForOption(_migrateArguments.OrgMetadataOnlyOption);

        var intervalArgs = new IntervalArgsBinder(_intervalArguments).Get(bindingContext);

        return new MigrateArgs(
            repositories,
            lockRepositories,
            excludeMetadata,
            excludeGitData,
            excludeAttachements,
            excludeReleases,
            excludeOwnerProjects,
            excludeMetadataOnly,
            intervalArgs
        );
    }
}