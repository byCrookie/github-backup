using System.CommandLine.Binding;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class MigrationsArgsBinder : BinderBase<MigrationsArgs>
{
    private readonly MigrationArguments _migrationArguments;

    public MigrationsArgsBinder(MigrationArguments migrationArguments)
    {
        _migrationArguments = migrationArguments;
    }
    
    protected override MigrationsArgs GetBoundValue(BindingContext bindingContext)
    {
        var id = bindingContext.ParseResult.GetRequiredValueForOption(_migrationArguments.IdOption);
        return new MigrationsArgs(id);
    }
}