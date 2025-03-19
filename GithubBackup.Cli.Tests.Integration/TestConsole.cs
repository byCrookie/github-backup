using System.CommandLine.IO;
using System.Text;
using GithubBackup.Cli.Console;
using Spectre.Console;

namespace GithubBackup.Cli.Tests.Integration;

public class TestConsole : ICliConsole
{
    private readonly Spectre.Console.Testing.TestConsole _testConsole;

    public TestConsole(Spectre.Console.Testing.TestConsole testConsole)
    {
        _testConsole = testConsole;

        var writer = new StandardStreamWriter(testConsole);

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
        private readonly Spectre.Console.Testing.TestConsole _ansiConsole;

        public StandardStreamWriter(Spectre.Console.Testing.TestConsole ansiConsole)
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

        public override string ToString() => _ansiConsole.Output;
    }

    public void WriteException(Exception exception)
    {
        _testConsole.Markup($"[red]{exception.Message}[/]");
    }
}
