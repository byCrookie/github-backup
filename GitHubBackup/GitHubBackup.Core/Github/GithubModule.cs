using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Github;

internal static class GithubModule
{
   public static void AddGithub(this IServiceCollection services)
   {
      services.AddTransient<IGithubService, GithubService>();
   }
}