using System.CommandLine;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Download;

public class DownloadArguments(bool piping)
{
    public Option<long[]> MigrationsOption { get; } =
        new(name: "--migrations", aliases: ["-m"])
        {
            Required = false,
            Description = DownloadArgDescriptions.Migrations.Long,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = true,
            DefaultValueFactory = _ => Piping.ReadLongs(Console.In, piping, false),
        };

    public Option<bool> LatestOption { get; } =
        new(name: "--latest", aliases: ["-l"])
        {
            Required = false,
            Description = DownloadArgDescriptions.Latest.Long,
            DefaultValueFactory = _ => false,
        };

    public Option<bool> PollOption { get; } =
        new(name: "--poll", aliases: ["-p"])
        {
            Required = false,
            Description = DownloadArgDescriptions.Poll.Long,
            DefaultValueFactory = _ => false,
        };

    public Option<DirectoryInfo> DestinationOption { get; } =
        new(name: "--destination", aliases: ["-d"])
        {
            Required = true,
            Description = DownloadArgDescriptions.Destination.Long,
        };

    public Option<int?> NumberOfBackupsOption { get; } =
        new(name: "--number-of-backups", aliases: ["-n"])
        {
            Required = false,
            Description = DownloadArgDescriptions.NumberOfBackups.Long,
        };

    public Option<bool> OverwriteOption { get; } =
        new(name: "--overwrite", aliases: ["-o"])
        {
            Required = false,
            Description = DownloadArgDescriptions.Overwrite.Long,
            DefaultValueFactory = _ => true,
        };

    public List<Option> Options()
    {
        return
        [
            MigrationsOption,
            LatestOption,
            PollOption,
            DestinationOption,
            NumberOfBackupsOption,
            OverwriteOption,
        ];
    }
}
