using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Interval;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class DowndloadArgsBinder(
    DownloadArguments downloadArguments,
    IntervalArguments intervalArguments,
    LoginArguments loginArguments
)
{
    public DownloadArgs Get(ParseResult parseResult)
    {
        var migrations = parseResult.GetRequiredValue(downloadArguments.MigrationsOption);
        var latest = parseResult.GetRequiredValue(downloadArguments.LatestOption);
        var poll = parseResult.GetRequiredValue(downloadArguments.PollOption);
        var destination = parseResult.GetRequiredValue(downloadArguments.DestinationOption);
        var numberOfBackups = parseResult.GetValue(downloadArguments.NumberOfBackupsOption);
        var overwrite = parseResult.GetRequiredValue(downloadArguments.OverwriteOption);

        var intervalArgs = new IntervalArgsBinder(intervalArguments).Get(parseResult);
        var loginArgs = new LoginArgsBinder(loginArguments).Get(parseResult);

        return new DownloadArgs(
            migrations,
            latest,
            poll,
            destination,
            numberOfBackups,
            overwrite,
            intervalArgs,
            loginArgs
        );
    }
}
