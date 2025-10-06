using System.CommandLine;

namespace GithubBackup.TestUtils;

public static class ParseResultExtensions
{
    public static async Task<int> InvokeTestAsync(this ParseResult parseResult)
    {
        var exitCode = await parseResult.InvokeAsync(new InvocationConfiguration
        {
            EnableDefaultExceptionHandler = false
        });

        if (exitCode == 0) return exitCode;
        var errors = string.Join(Environment.NewLine, parseResult.Errors.Select(e => e.Message));
        throw new Exception($"Command failed with exit code {exitCode}:{Environment.NewLine}{errors}");
    }
}