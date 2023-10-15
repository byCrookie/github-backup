using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Utils;

public static class UtilsModule
{
    public static void AddUtils(this IServiceCollection services)
    {
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        services.AddTransient<IDateTimeOffsetProvider, DateTimeOffsetProvider>();
    }
}