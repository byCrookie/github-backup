using System.Runtime.Serialization;

namespace GithubBackup.Core.Github.Migrations;

public enum MigrationState
{
    [EnumMember(Value = "pending")]
    Pending,

    [EnumMember(Value = "exporting")]
    Exporting,

    [EnumMember(Value = "failed")]
    Failed,

    [EnumMember(Value = "exported")]
    Exported,
}
