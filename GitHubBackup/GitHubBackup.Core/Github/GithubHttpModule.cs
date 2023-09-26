using GitHubBackup.Core.Github.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubBackup.Core.Github;

internal static class GithubModule
{
   public static IServiceCollection AddGithub(this IServiceCollection services)
   {
      services.AddGithubHttp();
      services.AddTransient<IGithubService, GithubService>();
      return services;
   }
}