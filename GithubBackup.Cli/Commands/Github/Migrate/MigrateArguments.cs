using System.CommandLine;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrate;

public class MigrateArguments
{
    public Option<string[]> RepositoriesOption { get; } 
    public Option<bool> LockRepositoriesOption { get; }
    public Option<bool> ExcludeMetadataOption { get; }
    public Option<bool> ExcludeGitDataOption { get; }
    public Option<bool> ExcludeAttachementsOption { get; }
    public Option<bool> ExcludeReleasesOption { get; }
    public Option<bool> ExcludeOwnerProjectsOption { get; }
    public Option<bool> OrgMetadataOnlyOption { get; }

    public MigrateArguments(bool piping)
    {
        RepositoriesOption = new Option<string[]>(
            aliases: new[] { "-r", "--repositories" },
            getDefaultValue: () => Piping.ReadStrings(System.Console.In, piping, false),
            description: MigrateArgDescriptions.Repositories.Long
        ) { IsRequired = false, Arity = ArgumentArity.ZeroOrMore, AllowMultipleArgumentsPerToken = true };

        LockRepositoriesOption = new Option<bool>(
            aliases: new[] { "-lr", "--lock-repositories" },
            getDefaultValue: () => false,
            description: MigrateArgDescriptions.LockRepositories.Long
        ) { IsRequired = false };

        ExcludeMetadataOption = new Option<bool>(
            aliases: new[] { "-em", "--exclude-metadata" },
            getDefaultValue: () => false,
            description: MigrateArgDescriptions.ExcludeMetadata.Long
        ) { IsRequired = false };

        ExcludeGitDataOption = new Option<bool>(
            aliases: new[] { "-egd", "--exclude-git-data" },
            getDefaultValue: () => false,
            description: MigrateArgDescriptions.ExcludeGitData.Long
        ) { IsRequired = false };

        ExcludeAttachementsOption = new Option<bool>(
            aliases: new[] { "-ea", "--exclude-attachements" },
            getDefaultValue: () => false,
            description: MigrateArgDescriptions.ExcludeAttachements.Long
        ) { IsRequired = false };

        ExcludeReleasesOption = new Option<bool>(
            aliases: new[] { "-er", "--exclude-releases" },
            getDefaultValue: () => false,
            description: MigrateArgDescriptions.ExcludeReleases.Long
        ) { IsRequired = false };

        ExcludeOwnerProjectsOption = new Option<bool>(
            aliases: new[] { "-eop", "--exclude-owner-projects" },
            getDefaultValue: () => false,
            description: MigrateArgDescriptions.ExcludeOwnerProjects.Long
        ) { IsRequired = false };

        OrgMetadataOnlyOption = new Option<bool>(
            aliases: new[] { "-omo", "--org-metadata-only" },
            getDefaultValue: () => false,
            description: MigrateArgDescriptions.OrgMetadataOnly.Long
        ) { IsRequired = false };

        OrgMetadataOnlyOption.AddValidator(result =>
        {
            var repositories = result.GetValueForOption(RepositoriesOption);
            var orgMetadataOnly = result.GetValueForOption(OrgMetadataOnlyOption);

            if (orgMetadataOnly && repositories?.Length > 0)
            {
                result.ErrorMessage = MigrateArgsValidator.OrgMetadataOnlyMustBeUsedAlone;
            }
        });
    }
    
    public IEnumerable<Option> Options()
    {
        return new Option[]
        {
            RepositoriesOption,
            LockRepositoriesOption,
            ExcludeMetadataOption,
            ExcludeGitDataOption,
            ExcludeAttachementsOption,
            ExcludeReleasesOption,
            ExcludeOwnerProjectsOption,
            OrgMetadataOnlyOption
        };
    }
}