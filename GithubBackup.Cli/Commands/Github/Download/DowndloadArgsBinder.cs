﻿using System.CommandLine.Binding;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class DowndloadArgsBinder : BinderBase<DownloadArgs>
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

    public DownloadArgs Get(BindingContext bindingContext) => GetBoundValue(bindingContext);

    protected override DownloadArgs GetBoundValue(BindingContext bindingContext)
    {
        var migrations = bindingContext.ParseResult.GetRequiredValueForOption(
            _downloadArguments.MigrationsOption
        );
        var latest = bindingContext.ParseResult.GetRequiredValueForOption(
            _downloadArguments.LatestOption
        );
        var poll = bindingContext.ParseResult.GetRequiredValueForOption(
            _downloadArguments.PollOption
        );
        var destination = bindingContext.ParseResult.GetRequiredValueForOption(
            _downloadArguments.DestinationOption
        );
        var numberOfBackups = bindingContext.ParseResult.GetValueForOption(
            _downloadArguments.NumberOfBackupsOption
        );
        var overwrite = bindingContext.ParseResult.GetRequiredValueForOption(
            _downloadArguments.OverwriteOption
        );

        var intervalArgs = new IntervalArgsBinder(_intervalArguments).Get(bindingContext);
        var loginArgs = new LoginArgsBinder(_loginArguments).Get(bindingContext);

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
