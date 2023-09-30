using GithubBackup.Core.Github.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Github;

internal static class GithubModule
{
   public static void AddGithub(this IServiceCollection services)
   {
      services.AddGithubHttp();
      services.AddTransient<IGithubService, GithubService>();
   }
}