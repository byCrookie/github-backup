using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Repositories;
using GithubBackup.Core.Github.Users;
using StrongInject;


namespace GithubBackup.Core.Github;

[RegisterModule(typeof(MigrationModule))]
[RegisterModule(typeof(Authentication.AuthenticationModule))]
[RegisterModule(typeof(UserModule))]
[RegisterModule(typeof(RepositoryModule))]
public class GithubModule
{
}