using GithubBackup.Cli.Commands.Github.Credentials;
using StrongInject;


namespace GithubBackup.Cli.Commands.Github;

[RegisterModule(typeof(CredentialsModule))]
[Register<Backup>]
internal class GithubModule
{
}