using System.CommandLine.Binding;

namespace GithubBackup.Cli.Commands.Github.Interval;

internal sealed class IntervalArgsBinder : BinderBase<IntervalArgs>
{
    private readonly IntervalArguments _intervalArguments;

    public IntervalArgsBinder(IntervalArguments intervalArguments)
    {
        _intervalArguments = intervalArguments;
    }
    
    public IntervalArgs Get(BindingContext bindingContext) => GetBoundValue(bindingContext);
    
    protected override IntervalArgs GetBoundValue(BindingContext bindingContext)
    {
        var interval = bindingContext.ParseResult.GetValueForOption(_intervalArguments.IntervalOption);
        return new IntervalArgs(interval is null ? null : TimeSpan.FromSeconds(interval.Value));
    }
}