﻿using System.CommandLine.Binding;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class MigrationsArgsBinder : BinderBase<MigrationsArgs>
{
    protected override MigrationsArgs GetBoundValue(BindingContext bindingContext)
    {
        var id = bindingContext.ParseResult.GetRequiredValueForOption(MigrationsArgs.IdOption);
        return new MigrationsArgs(id);
    }
}