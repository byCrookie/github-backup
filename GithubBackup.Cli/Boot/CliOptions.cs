using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace GithubBackup.Cli.Boot;

public class CliOptions
{
    public bool EnableDefaultExceptionHandler { get; set; } = true;
    public TextWriter Output { get; init; } = AnsiConsole.Profile.Out.Writer;
    public TextWriter Error { get; init; } = AnsiConsole.Profile.Out.Writer;
    public Action<HostApplicationBuilder> AfterServices { get; init; } = _ => { };
}
