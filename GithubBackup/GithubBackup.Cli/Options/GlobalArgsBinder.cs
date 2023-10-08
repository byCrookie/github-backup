using System.CommandLine.Binding;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Options;

internal class GlobalArgsBinder : BinderBase<GlobalArgs>
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