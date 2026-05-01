using Microsoft.AspNetCore.Authorization;

namespace Identity.CustomPolicy
{
public class AllowUserPolicy : IAuthorizationRequirement
{
    public string[] AllowUsers { get; set; }

    public AllowUserPolicy(params string[] allowUsers)
    {
        AllowUsers=allowUsers;
    }
}
}
