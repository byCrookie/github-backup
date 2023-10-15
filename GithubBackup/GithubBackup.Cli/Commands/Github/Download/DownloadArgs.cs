﻿using System.CommandLine;

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
        if (!migrations.Any())
        {
            throw new ArgumentException("At least one migration must be specified.");
        }
        
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
            getDefaultValue: Array.Empty<long>,
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