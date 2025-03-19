using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Github.Users;

internal static class UserModule
{
    public static void AddUser(this IServiceCollection services)
    {
        services.AddTransient<IUserService, UserService>();
    }
}
