using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Interval;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class DowndloadArgsBinder
{
    private readonly DownloadArguments _downloadArguments;
    private readonly IntervalArguments _intervalArguments;
    private readonly LoginArguments _loginArguments;

    public DowndloadArgsBinder(
        DownloadArguments downloadArguments,
        IntervalArguments intervalArguments,
        LoginArguments loginArguments
    )
    {
        _downloadArguments = downloadArguments;
        _intervalArguments = intervalArguments;
        _loginArguments = loginArguments;
    }

    public DownloadArgs Get(ParseResult parseResult)
    {
        var migrations = parseResult.GetRequiredValue(
            _downloadArguments.MigrationsOption
        );
        var latest = parseResult.GetRequiredValue(
            _downloadArguments.LatestOption
        );
        var poll = parseResult.GetRequiredValue(
            _downloadArguments.PollOption
        );
        var destination = parseResult.GetRequiredValue(
            _downloadArguments.DestinationOption
        );
        var numberOfBackups = parseResult.GetValue(
            _downloadArguments.NumberOfBackupsOption
        );
        var overwrite = parseResult.GetRequiredValue(
            _downloadArguments.OverwriteOption
        );

        var intervalArgs = new IntervalArgsBinder(_intervalArguments).Get(parseResult);
        var loginArgs = new LoginArgsBinder(_loginArguments).Get(parseResult);

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
