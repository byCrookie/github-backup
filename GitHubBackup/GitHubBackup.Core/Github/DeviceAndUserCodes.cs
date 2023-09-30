namespace GithubBackup.Core.Github;

public record DeviceAndUserCodes(string DeviceCode, string UserCode, string VerificationUri, int ExpiresIn, int Interval);