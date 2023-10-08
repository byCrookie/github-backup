using System.CommandLine.Binding;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrate;

public class MigrateArgsBinder : BinderBase<MigrateArgs>
{
    protected override MigrateArgs GetBoundValue(BindingContext bindingContext)
    {
        var repositories = bindingContext.ParseResult.GetRequiredValueForOption(MigrateArgs.RepositoriesOption);
        var lockRepositories = bindingContext.ParseResult.GetRequiredValueForOption(MigrateArgs.LockRepositoriesOption);
        var excludeMetadata = bindingContext.ParseResult.GetRequiredValueForOption(MigrateArgs.ExcludeMetadataOption);
        var excludeGitData = bindingContext.ParseResult.GetRequiredValueForOption(MigrateArgs.ExcludeGitDataOption);
        var excludeAttachements = bindingContext.ParseResult.GetRequiredValueForOption(MigrateArgs.ExcludeAttachementsOption);
        var excludeReleases = bindingContext.ParseResult.GetRequiredValueForOption(MigrateArgs.ExcludeReleasesOption);
        var excludeOwnerProjects = bindingContext.ParseResult.GetRequiredValueForOption(MigrateArgs.ExcludeOwnerProjectsOption);
        var excludeMetadataOnly = bindingContext.ParseResult.GetRequiredValueForOption(MigrateArgs.ExcludeMetadataOnlyOption);

        return new MigrateArgs(
            repositories,
            lockRepositories,
            excludeMetadata,
            excludeGitData,
            excludeAttachements,
            excludeReleases,
            excludeOwnerProjects,
            excludeMetadataOnly
        );
    }
}