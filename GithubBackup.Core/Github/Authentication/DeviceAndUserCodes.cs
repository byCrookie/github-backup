namespace GithubBackup.Core.Github.Authentication;

public record DeviceAndUserCodes(string DeviceCode, string UserCode, string VerificationUri, int ExpiresIn, int Interval);