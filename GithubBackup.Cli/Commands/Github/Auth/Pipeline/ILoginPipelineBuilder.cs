namespace GithubBackup.Cli.Commands.Github.Auth.Pipeline;

internal interface ILoginPipelineBuilder
{
    public ILoginPipeline PersistedOnly();
    public ILoginPipeline WithPersistent();
    public ILoginPipeline WithoutPersistent();
}