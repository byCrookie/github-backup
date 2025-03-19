using System.ComponentModel;
using System.Runtime.Serialization;

namespace GithubBackup.Core.Github.Repositories;

public enum RepositoryAffiliation
{
    [EnumMember(Value = "owner")]
    [Description("Repositories that are owned by the authenticated user.")]
    Owner,

    [EnumMember(Value = "collaborator")]
    [Description("Repositories that the user has been added to as a collaborator.")]
    Collaborator,

    [EnumMember(Value = "organization_member")]
    [Description(
        "Repositories that the user has access to through being a member of an organization. This includes every repository on every team that the user is on."
    )]
    OrganizationMember,
}
