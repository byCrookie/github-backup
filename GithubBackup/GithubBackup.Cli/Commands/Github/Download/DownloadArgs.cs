using System.CommandLine;
using System.IO.Abstractions;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class DownloadArgs
{
    public int[] Migrations { get; }
    public bool Latest { get; }
    public IDirectoryInfo Destination { get; }
    public int? NumberOfBackups { get; }
    public bool Overwrite { get; }

    public DownloadArgs(
        int[] migrations,
        bool latest,
        IDirectoryInfo destination,
        int? numberOfBackups,
        bool overwrite)
    {
        Migrations = migrations;
        Latest = latest;
        Destination = destination;
        NumberOfBackups = numberOfBackups;
        Overwrite = overwrite;
    }

    public static Option<int[]> MigrationsOption { get; }
    public static Option<bool> LatestOption { get; }
    public static Option<IDirectoryInfo> DestinationOption { get; }
    public static Option<int?> NumberOfBackupsOption { get; }
    public static Option<bool> OverwriteOption { get; }

    static DownloadArgs()
    {
        MigrationsOption = new Option<int[]>(
            aliases: new[] { "-m", "--migrations" },
            description: DownloadArgDescriptions.Migrations.Long
        ) { IsRequired = false };

        LatestOption = new Option<bool>(
            aliases: new[] { "-l", "--latest" },
            description: DownloadArgDescriptions.Latest.Long
        ) { IsRequired = false };

        DestinationOption = new Option<IDirectoryInfo>(
            aliases: new[] { "-d", "--destination" },
            description: DownloadArgDescriptions.Destination.Long
        ) { IsRequired = true };

        NumberOfBackupsOption = new Option<int?>(
            aliases: new[] { "-n", "--number-of-backups" },
            description: DownloadArgDescriptions.NumberOfBackups.Long
        ) { IsRequired = false };

        OverwriteOption = new Option<bool>(
            aliases: new[] { "-o", "--overwrite" },
            getDefaultValue: () => true,
            description: DownloadArgDescriptions.Overwrite.Long
        ) { IsRequired = false };

        LatestOption.AddValidator(result =>
        {
            var migrations = result.GetValueForOption(MigrationsOption);
            var latest = result.GetValueForOption(LatestOption);

            if (latest && migrations?.Length > 0)
            {
                result.ErrorMessage = "The '-l / --latest' option cannot be used with the '-m / --migrations' option.";
            }
        });
    }
}