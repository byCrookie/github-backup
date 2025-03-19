using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.DependencyInjection.Factory;

internal static class FactoryModule
{
    public static void AddFactory(this IServiceCollection services)
    {
        services.AddTransient<IFactory, Factory>();
        services.AddTransient(typeof(IFactory<>), typeof(Factory<>));
        services.AddTransient(typeof(IFactory<,>), typeof(Factory<,>));
        services.AddTransient(typeof(IFactory<,,>), typeof(Factory<,,>));
        services.AddTransient(typeof(IFactory<,,,>), typeof(Factory<,,,>));
        services.AddTransient(typeof(IFactory<,,,,>), typeof(Factory<,,,,>));
        services.AddTransient(typeof(IFactory<,,,,,>), typeof(Factory<,,,,,>));
    }
}
