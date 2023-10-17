using System.CommandLine;
using FluentValidation;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal sealed class MigrateArgs
{
    public string[] Repositories { get; }
    public bool LockRepositories { get; }
    public bool ExcludeMetadata { get; }
    public bool ExcludeGitData { get; }
    public bool ExcludeAttachements { get; }
    public bool ExcludeReleases { get; }
    public bool ExcludeOwnerProjects { get; }
    public bool OrgMetadataOnly { get; }

    public MigrateArgs(
        string[] repositories,
        bool lockRepositories,
        bool excludeMetadata,
        bool excludeGitData,
        bool excludeAttachements,
        bool excludeReleases,
        bool excludeOwnerProjects,
        bool orgMetadataOnly
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
        
        new MigrateArgsValidator().ValidateAndThrow(this);
    }

    public static Option<string[]> RepositoriesOption { get; }
    public static Option<bool> LockRepositoriesOption { get; }
    public static Option<bool> ExcludeMetadataOption { get; }
    public static Option<bool> ExcludeGitDataOption { get; }
    public static Option<bool> ExcludeAttachementsOption { get; }
    public static Option<bool> ExcludeReleasesOption { get; }
    public static Option<bool> ExcludeOwnerProjectsOption { get; }
    public static Option<bool> OrgMetadataOnlyOption { get; }

    static MigrateArgs()
    {
        RepositoriesOption = new Option<string[]>(
            aliases: new[] { "-r", "--repositories" },
            getDefaultValue: StandardInput.ReadStrings,
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
    
    public static Option[] Options()
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