using System.CommandLine;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Download;

public class DownloadArguments
{
    public Option<long[]> MigrationsOption { get; }
    public Option<bool> LatestOption { get; }
    public Option<bool> PollOption { get; }
    public Option<DirectoryInfo> DestinationOption { get; }
    public Option<int?> NumberOfBackupsOption { get; }
    public Option<bool> OverwriteOption { get; }

    public DownloadArguments(bool piping)
    {
        MigrationsOption = new Option<long[]>(
            aliases: new[] { "-m", "--migrations" },
            getDefaultValue: () => Piping.ReadLongs(System.Console.In, piping, false),
            description: DownloadArgDescriptions.Migrations.Long
        )
        {
            IsRequired = false,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true,
        };

        LatestOption = new Option<bool>(
            aliases: new[] { "-l", "--latest" },
            getDefaultValue: () => false,
            description: DownloadArgDescriptions.Latest.Long
        )
        {
            IsRequired = false,
        };

        PollOption = new Option<bool>(
            aliases: new[] { "-p", "--poll" },
            getDefaultValue: () => false,
            description: DownloadArgDescriptions.Poll.Long
        )
        {
            IsRequired = false,
        };

        DestinationOption = new Option<DirectoryInfo>(
            aliases: new[] { "-d", "--destination" },
            description: DownloadArgDescriptions.Destination.Long
        )
        {
            IsRequired = true,
        };

        NumberOfBackupsOption = new Option<int?>(
            aliases: new[] { "-n", "--number-of-backups" },
            description: DownloadArgDescriptions.NumberOfBackups.Long
        )
        {
            IsRequired = false,
        };

        OverwriteOption = new Option<bool>(
            aliases: new[] { "-o", "--overwrite" },
            getDefaultValue: () => true,
            description: DownloadArgDescriptions.Overwrite.Long
        )
        {
            IsRequired = false,
        };
    }

    public List<Option> Options()
    {
        return new List<Option>
        {
            MigrationsOption,
            LatestOption,
            PollOption,
            DestinationOption,
            NumberOfBackupsOption,
            OverwriteOption,
        };
    }
}
