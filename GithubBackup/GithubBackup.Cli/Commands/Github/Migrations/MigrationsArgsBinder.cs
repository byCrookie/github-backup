using System.CommandLine.Binding;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class MigrationsArgsBinder : BinderBase<MigrationsArgs>
{
    private readonly MigrationsArguments _migrationsArguments;

    public MigrationsArgsBinder(MigrationsArguments migrationsArguments)
    {
        _migrationsArguments = migrationsArguments;
    }
    
    protected override MigrationsArgs GetBoundValue(BindingContext bindingContext)
    {
        var export = bindingContext.ParseResult.GetRequiredValueForOption(_migrationsArguments.ExportOption);
        var since = bindingContext.ParseResult.GetValueForOption(_migrationsArguments.SinceOption);
        var daysOld = bindingContext.ParseResult.GetValueForOption(_migrationsArguments.DaysOldOption);
        return new MigrationsArgs(export, daysOld, since);
    }
}