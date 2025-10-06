using System.CommandLine;
using GithubBackup.Core.Github.Repositories;

namespace GithubBackup.Cli.Commands.Github.Repositories;

public class RepositoriesArguments
{
    public Option<RepositoryType?> TypeOption { get; } =
        new(
            name: "--type",
            aliases: ["-t"]
        )
        {
            Required = false,
            Description = RepositoriesArgDescriptions.Type.Long
        };

    public Option<RepositoryAffiliation?> AffiliationOption { get; } =
        new(
            name: "--affiliation",
            aliases: ["-a"]
        )
        {
            Required = false,
            Description = RepositoriesArgDescriptions.Affiliation.Long,
            DefaultValueFactory = _ => RepositoryAffiliation.Owner
        };

    public Option<RepositoryVisibility?> VisibilityOption { get; } =
        new(
            name: "--visibility",
            aliases: ["-v"]
        )
        {
            Required = false,
            Description = RepositoriesArgDescriptions.Visibility.Long,
            DefaultValueFactory = _ => RepositoryVisibility.All
        };

    public IEnumerable<Option> Options()
    {
        return new Option[] { TypeOption, AffiliationOption, VisibilityOption };
    }
}
