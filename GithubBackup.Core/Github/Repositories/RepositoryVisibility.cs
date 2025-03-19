using System.Runtime.Serialization;

namespace GithubBackup.Core.Github.Repositories;

public enum RepositoryVisibility
{
    [EnumMember(Value = "all")]
    All,

    [EnumMember(Value = "public")]
    Public,

    [EnumMember(Value = "private")]
    Private,
}
