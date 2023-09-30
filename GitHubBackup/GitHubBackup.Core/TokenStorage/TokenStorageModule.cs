using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.TokenStorage;

internal static class TokenStorageModule
{
   public static void AddTokenStorage(this IServiceCollection services)
   {
      services.AddTransient<ITokenStorageService, TokenStorageService>();
   }
}