using Microsoft.AspNetCore.Authorization;
using MyUMCApp.Shared.Models;

namespace MyUMCApp.Client.Auth;

public static class AuthorizationPolicies
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireDeveloper = "RequireDeveloper";
    public const string RequireChurchLeader = "RequireChurchLeader";
    public const string RequireMember = "RequireMember";
    public const string RequireActiveUser = "RequireActiveUser";
    public const string RequireVerifiedEmail = "RequireVerifiedEmail";

    public static void AddAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(RequireAdmin, policy =>
            policy.RequireClaim("user_type", UserType.Administrator.ToString()));

        options.AddPolicy(RequireDeveloper, policy =>
            policy.RequireClaim("user_type", UserType.Developer.ToString()));

        options.AddPolicy(RequireChurchLeader, policy =>
            policy.RequireClaim("user_type", UserType.ChurchLeader.ToString()));

        options.AddPolicy(RequireMember, policy =>
            policy.RequireClaim("user_type", UserType.Member.ToString(), UserType.ChurchLeader.ToString()));

        options.AddPolicy(RequireActiveUser, policy =>
            policy.RequireClaim("is_active", "true"));

        options.AddPolicy(RequireVerifiedEmail, policy =>
            policy.RequireClaim("email_verified", "true"));
    }
} 