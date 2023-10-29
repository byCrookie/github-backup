using System.CommandLine;
using System.CommandLine.IO;
using System.Text;

namespace GithubBackup.Cli.Tests.Integration;

public class TestConsole : IConsole
{
    public TestConsole()
    {
        var writer = new StandardStreamWriter();
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
        private readonly StringBuilder _stringBuilder = new();

        public override void Write(char value)
        {
            _stringBuilder.Append(value);
        }

        public override void Write(string? value)
        {
            _stringBuilder.Append(value);
        }

        public override Encoding Encoding { get; } = Encoding.Unicode;

        public override string ToString() => _stringBuilder.ToString();
    }
}