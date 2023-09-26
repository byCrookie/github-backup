using GitHubBackup.Cli.Github.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubBackup.Cli.Github;

internal static class GithubModule
{
   public static IServiceCollection AddGithub(this IServiceCollection services)
   {
      services.AddGithubHttp();
      services.AddTransient<IGithubService, GithubService>();
      return services;
   }
}