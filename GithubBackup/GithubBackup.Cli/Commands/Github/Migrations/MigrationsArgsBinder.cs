using System.CommandLine.Binding;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class MigrationsArgsBinder : BinderBase<MigrationsArgs>
{
    private readonly MigrationsArguments _migrationsArguments;
    private readonly LoginArguments _loginArguments;

    public MigrationsArgsBinder(
        MigrationsArguments migrationsArguments,
        LoginArguments loginArguments
    )
    {
        _migrationsArguments = migrationsArguments;
        _loginArguments = loginArguments;
    }

    protected override MigrationsArgs GetBoundValue(BindingContext bindingContext)
    {
        var export = bindingContext.ParseResult.GetRequiredValueForOption(_migrationsArguments.ExportOption);
        var since = bindingContext.ParseResult.GetValueForOption(_migrationsArguments.SinceOption);
        var daysOld = bindingContext.ParseResult.GetValueForOption(_migrationsArguments.DaysOldOption);

        var loginArgs = new LoginArgsBinder(_loginArguments).Get(bindingContext);

        return new MigrationsArgs(export, daysOld, since, loginArgs);
    }
}