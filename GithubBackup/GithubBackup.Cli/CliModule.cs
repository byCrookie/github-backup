using GithubBackup.Cli.Commands.Github;
using GithubBackup.Cli.Commands.Services;
using GithubBackup.Core;
using StrongInject;

namespace GithubBackup.Cli;

[RegisterModule(typeof(CoreModule))]
[RegisterModule(typeof(GithubModule))]
[RegisterModule(typeof(ServicesModule))]
internal class CliModule
{
    
}