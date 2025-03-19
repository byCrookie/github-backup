namespace GithubBackup.Core.Github.Authentication;

public record AccessToken(string Token, string TokenType, string Scope);
