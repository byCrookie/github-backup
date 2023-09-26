using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace GitHubBackup.Core.Github.Http;

internal static class GithubHttpModule
{
   public static IServiceCollection AddGithubHttp(this IServiceCollection services)
   {
      var retryPolicy = HttpPolicyExtensions
         .HandleTransientHttpError()
         .Or<TimeoutRejectedException>()
         .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1));

      var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);  

      services.AddHttpClient(GithubHttpClient.Name, httpClient =>
         {
            httpClient.BaseAddress = new Uri("https://api.github.com/");
            httpClient.DefaultRequestHeaders.Add(
               HeaderNames.Accept, "application/vnd.github.v3+json");
            httpClient.DefaultRequestHeaders.Add(
               HeaderNames.UserAgent, "GithubBackup.Cli");
         })
         .AddPolicyHandler(retryPolicy)
         .AddPolicyHandler(timeoutPolicy);
      
      return services;
   }
}