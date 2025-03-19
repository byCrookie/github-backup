using System.Runtime.Serialization;

namespace GithubBackup.Core.Github.Repositories;

public enum RepositoryType
{
    [EnumMember(Value = "all")]
    All,

    [EnumMember(Value = "public")]
    Public,

    [EnumMember(Value = "private")]
    Private,

    [EnumMember(Value = "forks")]
    Forks,

    [EnumMember(Value = "sources")]
    Sources,

    [EnumMember(Value = "member")]
    Member,
}
