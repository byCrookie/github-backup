using System.CommandLine.Binding;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Download;

internal sealed class DowndloadArgsBinder : BinderBase<DownloadArgs>
{
    private readonly DownloadArguments _downloadArguments;

    public DowndloadArgsBinder(DownloadArguments downloadArguments)
    {
        _downloadArguments = downloadArguments;
    }
    
    public DownloadArgs Get(BindingContext bindingContext) => GetBoundValue(bindingContext);

    protected override DownloadArgs GetBoundValue(BindingContext bindingContext)
    {
        var migrations = bindingContext.ParseResult.GetRequiredValueForOption(_downloadArguments.MigrationsOption);
        var latest = bindingContext.ParseResult.GetRequiredValueForOption(_downloadArguments.LatestOption);
        var destination = bindingContext.ParseResult.GetRequiredValueForOption(_downloadArguments.DestinationOption);
        var numberOfBackups = bindingContext.ParseResult.GetValueForOption(_downloadArguments.NumberOfBackupsOption);
        var overwrite = bindingContext.ParseResult.GetRequiredValueForOption(_downloadArguments.OverwriteOption);

        return new DownloadArgs(
            migrations,
            latest,
            destination,
            numberOfBackups,
            overwrite
        );
    }
}