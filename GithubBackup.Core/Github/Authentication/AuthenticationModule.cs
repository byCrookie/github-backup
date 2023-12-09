using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Github.Authentication;

internal static class AuthenticationModule
{
   public static void AddAuthentication(this IServiceCollection services)
   {
      services.AddTransient<IAuthenticationService, AuthenticationService>();
   }
}