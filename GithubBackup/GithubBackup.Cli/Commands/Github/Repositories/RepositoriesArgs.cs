using System.CommandLine;
using GithubBackup.Core.Github.Repositories;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal sealed class RepositoriesArgs
{
    public RepositoryType? Type { get; }
    public RepositoryAffiliation? Affiliation { get; }
    public RepositoryVisibility? Visibility { get; }

    public RepositoriesArgs(
        RepositoryType? type = null, 
        RepositoryAffiliation? affiliation = RepositoryAffiliation.Owner,
        RepositoryVisibility? visibility = RepositoryVisibility.All)
    {
        Type = type;
        Affiliation = affiliation;
        Visibility = visibility;
    }

    public static Option<RepositoryType?> TypeOption { get; }
    public static Option<RepositoryAffiliation?> AffiliationOption { get; }
    public static Option<RepositoryVisibility?> VisibilityOption { get; }

    static RepositoriesArgs()
    {
        TypeOption = new Option<RepositoryType?>(
            aliases: new[] { "-t", "--type" },
            getDefaultValue: () => null,
            description: RepositoriesArgDescriptions.Type.Long
        ) { IsRequired = false };
        
        AffiliationOption = new Option<RepositoryAffiliation?>(
            aliases: new[] { "-a", "--affiliation" },
            getDefaultValue: () => RepositoryAffiliation.Owner,
            description: RepositoriesArgDescriptions.Affiliation.Long
        ) { IsRequired = false };
        
        VisibilityOption = new Option<RepositoryVisibility?>(
            aliases: new[] { "-v", "--visibility" },
            getDefaultValue: () => RepositoryVisibility.All,
            description: RepositoriesArgDescriptions.Visibility.Long
        ) { IsRequired = false };
        
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
}