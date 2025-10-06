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
            name: "--repositories",
            aliases: ["-r"]
        )
        {
            Required = false,
            Description = MigrateArgDescriptions.Repositories.Long,
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = true,
            DefaultValueFactory = _ => Piping.ReadStrings(Console.In, piping, false)
        };

        LockRepositoriesOption = new Option<bool>(
            name: "--lock-repositories",
            aliases: ["-lr"]
        )
        {
            Required = false,
            Description = MigrateArgDescriptions.LockRepositories.Long,
            DefaultValueFactory = _ => false
        };

        ExcludeMetadataOption = new Option<bool>(
            name: "--exclude-metadata",
            aliases: ["-em"]
        )
        {
            Required = false,
            Description = MigrateArgDescriptions.ExcludeMetadata.Long,
            DefaultValueFactory = _ => false
        };

        ExcludeGitDataOption = new Option<bool>(
            name: "--exclude-git-data",
            aliases: ["-egd"]
        )
        {
            Required = false,
            Description = MigrateArgDescriptions.ExcludeGitData.Long,
            DefaultValueFactory = _ => false
        };

        ExcludeAttachementsOption = new Option<bool>(
            name: "--exclude-attachements",
            aliases: ["-ea"]
        )
        {
            Required = false,
            Description = MigrateArgDescriptions.ExcludeAttachements.Long,
            DefaultValueFactory = _ => false
        };

        ExcludeReleasesOption = new Option<bool>(
            name: "--exclude-releases",
            aliases: ["-er"]
        )
        {
            Required = false,
            Description = MigrateArgDescriptions.ExcludeReleases.Long,
            DefaultValueFactory = _ => false
        };

        ExcludeOwnerProjectsOption = new Option<bool>(
            name: "--exclude-owner-projects",
            aliases: ["-eop"]
        )
        {
            Required = false,
            Description = MigrateArgDescriptions.ExcludeOwnerProjects.Long,
            DefaultValueFactory = _ => false
        };

        OrgMetadataOnlyOption = new Option<bool>(
            name: "--org-metadata-only",
            aliases: ["-omo"]
        )
        {
            Required = false,
            Description = MigrateArgDescriptions.OrgMetadataOnly.Long,
            DefaultValueFactory = _ => false
        };

        OrgMetadataOnlyOption.Validators.Add(result =>
        {
            var repositories = result.GetValue(RepositoriesOption);
            var orgMetadataOnly = result.GetValue(OrgMetadataOnlyOption);

            if (orgMetadataOnly && repositories?.Length > 0)
            {
                result.AddError(MigrateArgsValidator.OrgMetadataOnlyMustBeUsedAlone);
            }
        });
    }

    public IEnumerable<Option> Options()
    {
        return
        [
            RepositoriesOption,
            LockRepositoriesOption,
            ExcludeMetadataOption,
            ExcludeGitDataOption,
            ExcludeAttachementsOption,
            ExcludeReleasesOption,
            ExcludeOwnerProjectsOption,
            OrgMetadataOnlyOption
        ];
    }
}
