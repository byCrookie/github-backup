namespace GithubBackup.TestUtils.Env;

// ReSharper disable once InconsistentNaming
public static class OS
{
    public static string Description()
    {
        if (OperatingSystem.IsWindows())
            return "windows";
        if (OperatingSystem.IsLinux())
            return "linux";
        if (OperatingSystem.IsMacOS())
            return "macos";
        
        throw new NotSupportedException("Operating system not supported");
    }
}