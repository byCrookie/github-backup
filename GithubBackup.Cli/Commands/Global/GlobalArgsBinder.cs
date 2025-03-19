using System.CommandLine.Binding;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Global;

internal sealed class GlobalArgsBinder : BinderBase<GlobalArgs>
{
    private readonly GlobalArguments _globalArguments;

    public GlobalArgsBinder(GlobalArguments globalArguments)
    {
        _globalArguments = globalArguments;
    }

    protected override GlobalArgs GetBoundValue(BindingContext bindingContext)
    {
        var verbosity = bindingContext.ParseResult.GetRequiredValueForOption(
            _globalArguments.VerbosityOption
        );
        var quiet = bindingContext.ParseResult.GetRequiredValueForOption(
            _globalArguments.QuietOption
        );
        var logFile = bindingContext.ParseResult.GetValueForOption(_globalArguments.LogFileOption);
        return new GlobalArgs(verbosity, quiet, logFile);
    }
}
