using System.CommandLine.IO;
using System.Text;
using Spectre.Console;

namespace GithubBackup.Cli.Console;

public class CliConsole : ICliConsole
{
    private readonly IAnsiConsole _ansiConsole;

    public CliConsole(IAnsiConsole ansiConsole)
    {
        _ansiConsole = ansiConsole;
        
        var writer = new StandardStreamWriter(ansiConsole);
        
        Out = writer;
        Error = writer;
    }

    public IStandardStreamWriter Error { get; }
    public IStandardStreamWriter Out { get; }

    public bool IsOutputRedirected => false;
    public bool IsErrorRedirected => false;
    public bool IsInputRedirected => false;

    private class StandardStreamWriter : TextWriter, IStandardStreamWriter
    {
        private readonly IAnsiConsole _ansiConsole;

        public StandardStreamWriter(IAnsiConsole ansiConsole)
        {
            _ansiConsole = ansiConsole;
        }

        public override void Write(char value)
        {
            _ansiConsole.Profile.Out.Writer.Write(value);
        }

        public override void Write(string? value)
        {
            _ansiConsole.Profile.Out.Writer.Write(value);
        }

        public override Encoding Encoding { get; } = Encoding.UTF8;

        public override string ToString() => _ansiConsole.Profile.Out.ToString() ?? string.Empty;
    }

    public void WriteException(Exception exception)
    {
        _ansiConsole.Markup($"[red]{exception.Message}[/]");
    }
}