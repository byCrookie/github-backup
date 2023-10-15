using System.CommandLine.Binding;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class DowndloadArgsBinder : BinderBase<DownloadArgs>
{
    public DownloadArgs Get(BindingContext bindingContext) => GetBoundValue(bindingContext);

    protected override DownloadArgs GetBoundValue(BindingContext bindingContext)
    {
        var migrations = bindingContext.ParseResult.GetRequiredValueForOption(DownloadArgs.MigrationsOption);
        var latest = bindingContext.ParseResult.GetValueForOption(DownloadArgs.LatestOption);
        var destination = bindingContext.ParseResult.GetRequiredValueForOption(DownloadArgs.DestinationOption);
        var numberOfBackups = bindingContext.ParseResult.GetValueForOption(DownloadArgs.NumberOfBackupsOption);
        var overwrite = bindingContext.ParseResult.GetValueForOption(DownloadArgs.OverwriteOption);

        return new DownloadArgs(
            migrations.Any() ? migrations : StandardInput.ReadLongs(),
            latest,
            destination,
            numberOfBackups,
            overwrite
        );
    }
}