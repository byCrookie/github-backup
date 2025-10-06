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
            name: "--migrations",
            aliases: ["-m"]
        )
        {
            Required = false,
            Description = DownloadArgDescriptions.Migrations.Long,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true,
            DefaultValueFactory = _ => Piping.ReadLongs(System.Console.In, piping, false)
        };

        LatestOption = new Option<bool>(
            name: "--latest",
            aliases: ["-l"]
        )
        {
            Required = false,
            Description = DownloadArgDescriptions.Latest.Long,
            DefaultValueFactory = _ => false
        };

        PollOption = new Option<bool>(
            name: "--poll",
            aliases: ["-p"]
        )
        {
            Required = false,
            Description = DownloadArgDescriptions.Poll.Long,
            DefaultValueFactory = _ => false
        };

        DestinationOption = new Option<DirectoryInfo>(
            name: "--destination",
            aliases: ["-d"]
        )
        {
            Required = true,
            Description = DownloadArgDescriptions.Destination.Long
        };

        NumberOfBackupsOption = new Option<int?>(
            name: "--number-of-backups",
            aliases: ["-n"]
        )
        {
            Required = false,
            Description = DownloadArgDescriptions.NumberOfBackups.Long
        };

        OverwriteOption = new Option<bool>(
            name: "--overwrite",
            aliases: ["-o"]
        )
        {
            Required = false,
            Description = DownloadArgDescriptions.Overwrite.Long,
            DefaultValueFactory = _ => true
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
