using System.CommandLine;
using GithubBackup.Core.Github.Repositories;

namespace GithubBackup.Cli.Commands.Github.Repositories;

public class RepositoriesArguments
{
    public Option<RepositoryType?> TypeOption { get; } = new(
        aliases: new[] { "-t", "--type" },
        getDefaultValue: () => null,
        description: RepositoriesArgDescriptions.Type.Long
    ) { IsRequired = false };

    public Option<RepositoryAffiliation?> AffiliationOption { get; } = new(
        aliases: new[] { "-a", "--affiliation" },
        getDefaultValue: () => RepositoryAffiliation.Owner,
        description: RepositoriesArgDescriptions.Affiliation.Long
    ) { IsRequired = false };

    public Option<RepositoryVisibility?> VisibilityOption { get; } = new(
        aliases: new[] { "-v", "--visibility" },
        getDefaultValue: () => RepositoryVisibility.All,
        description: RepositoriesArgDescriptions.Visibility.Long
    ) { IsRequired = false };

    public RepositoriesArguments()
    {
        TypeOption.AddValidator(result =>
        {
            var type = result.GetValueForOption(TypeOption);
            var affiliation = result.GetValueForOption(AffiliationOption);
            var visibility = result.GetValueForOption(VisibilityOption);

            if (type is not null && (affiliation is not null || visibility is not null))
            {
                result.ErrorMessage = "The '-t / --type' option cannot be used with '-a / --affiliation' or '-v / --visibility' options.";
            }
        });
    }
    
    public IEnumerable<Option> Options()
    {
        return new Option[]
        {
            TypeOption,
            AffiliationOption,
            VisibilityOption
        };
    }
}