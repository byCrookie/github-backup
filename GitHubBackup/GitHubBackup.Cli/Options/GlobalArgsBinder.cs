using System.CommandLine.Binding;
using GitHubBackup.Cli.Utils;

namespace GitHubBackup.Cli.Options;

public class GlobalArgsBinder : BinderBase<GlobalArgs>
{
    protected override GlobalArgs GetBoundValue(BindingContext bindingContext)
    {
        var verbosity = bindingContext.ParseResult.GetRequiredValueForOption(GlobalArgs.VerbosityOption);
        var quiet = bindingContext.ParseResult.GetRequiredValueForOption(GlobalArgs.QuietOption);
        var logFile = bindingContext.ParseResult.GetValueForOption(GlobalArgs.LogFileOption);
        var interactive = bindingContext.ParseResult.GetRequiredValueForOption(GlobalArgs.InteractiveOption);
        return new GlobalArgs(verbosity, quiet, logFile, interactive);
    }
}