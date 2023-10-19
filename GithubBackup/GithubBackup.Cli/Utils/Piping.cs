namespace GithubBackup.Cli.Utils;

public static class Piping
{
    public static bool IsEnabled { get; set; } = true;
    public static Seperators Separators { get; } = new(new[]{ ",", ":", ";", "|", "-" });
    
    public static long[] ReadLongs()
    {
        if (!IsPipedInput())
        {
            return Array.Empty<long>();
        }
        
        var inputs = new List<long>();
        while (Console.ReadLine() is { } line && !string.IsNullOrEmpty(line))
        {
            var ints = line
                .Split(Separators.Values, StringSplitOptions.RemoveEmptyEntries)
                .Select(input => long.Parse(input.Trim()));

            inputs.AddRange(ints);
        }

        return !inputs.Any() ? Array.Empty<long>() : inputs.ToArray();
    }

    public static string[] ReadStrings()
    {
        if (!IsPipedInput())
        {
            return Array.Empty<string>();
        }

        var inputs = new List<string>();
        while (Console.ReadLine() is { } line && !string.IsNullOrEmpty(line))
        {
            var ints = line
                .Split(Separators.Values, StringSplitOptions.RemoveEmptyEntries)
                .Select(input => input.Trim());

            inputs.AddRange(ints);
        }

        return !inputs.Any() ? Array.Empty<string>() : inputs.ToArray();
    }

    private static bool IsPipedInput()
    {
        try
        {
            _ = Console.KeyAvailable;
            return false;
        }
        catch
        {
            return true;
        }
    }
}