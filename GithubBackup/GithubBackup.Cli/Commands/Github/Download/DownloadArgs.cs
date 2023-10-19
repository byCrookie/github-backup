using System.CommandLine;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class DownloadArgs
{
    public long[] Migrations { get; }
    public bool Latest { get; }
    public DirectoryInfo Destination { get; }
    public int? NumberOfBackups { get; }
    public bool Overwrite { get; }

    public DownloadArgs(
        long[] migrations,
        bool latest,
        DirectoryInfo destination,
        int? numberOfBackups,
        bool overwrite)
    {
        Migrations = migrations;
        Latest = latest;
        Destination = destination;
        NumberOfBackups = numberOfBackups;
        Overwrite = overwrite;
    }

    public static Option<long[]> MigrationsOption { get; }
    public static Option<bool> LatestOption { get; }
    public static Option<DirectoryInfo> DestinationOption { get; }
    public static Option<int?> NumberOfBackupsOption { get; }
    public static Option<bool> OverwriteOption { get; }

    static DownloadArgs()
    {
        MigrationsOption = new Option<long[]>(
            aliases: new[] { "-m", "--migrations" },
            getDefaultValue: Piping.ReadLongs,
            description: DownloadArgDescriptions.Migrations.Long)
        {
            IsRequired = false,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true
        };

        LatestOption = new Option<bool>(
            aliases: new[] { "-l", "--latest" },
            getDefaultValue: () => false,
            description: DownloadArgDescriptions.Latest.Long
        ) { IsRequired = false };

        DestinationOption = new Option<DirectoryInfo>(
            aliases: new[] { "-d", "--destination" },
            description: DownloadArgDescriptions.Destination.Long
        ) { IsRequired = true };

        NumberOfBackupsOption = new Option<int?>(
            aliases: new[] { "-n", "--number-of-backups" },
            getDefaultValue: () => null,
            description: DownloadArgDescriptions.NumberOfBackups.Long
        ) { IsRequired = false };

        OverwriteOption = new Option<bool>(
            aliases: new[] { "-o", "--overwrite" },
            getDefaultValue: () => true,
            description: DownloadArgDescriptions.Overwrite.Long
        ) { IsRequired = false };
    }
    
    public static Option[] Options()
    {
        return new Option[]
        {
            MigrationsOption,
            LatestOption,
            DestinationOption,
            NumberOfBackupsOption,
            OverwriteOption
        };
    }
}