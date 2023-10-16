using System.CommandLine.Binding;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class DowndloadArgsBinder : BinderBase<DownloadArgs>
{
    public DownloadArgs Get(BindingContext bindingContext) => GetBoundValue(bindingContext);

    protected override DownloadArgs GetBoundValue(BindingContext bindingContext)
    {
        var migrations = bindingContext.ParseResult.GetRequiredValueForOption(DownloadArgs.MigrationsOption);
        var latest = bindingContext.ParseResult.GetRequiredValueForOption(DownloadArgs.LatestOption);
        var destination = bindingContext.ParseResult.GetRequiredValueForOption(DownloadArgs.DestinationOption);
        var numberOfBackups = bindingContext.ParseResult.GetValueForOption(DownloadArgs.NumberOfBackupsOption);
        var overwrite = bindingContext.ParseResult.GetRequiredValueForOption(DownloadArgs.OverwriteOption);

        return new DownloadArgs(
            migrations,
            latest,
            destination,
            numberOfBackups,
            overwrite
        );
    }
}