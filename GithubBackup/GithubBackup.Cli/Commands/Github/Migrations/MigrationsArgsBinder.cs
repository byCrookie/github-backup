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
        var id = bindingContext.ParseResult.GetRequiredValueForOption(_migrationsArguments.LongOption);
        return new MigrationsArgs(id);
    }
}