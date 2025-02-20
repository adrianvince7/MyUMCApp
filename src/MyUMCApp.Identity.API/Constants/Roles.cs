namespace MyUMCApp.Identity.API.Constants;

public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Developer = "Developer";
    public const string Guest = "Guest";
    public const string Member = "Member";
    public const string ChurchLeader = "ChurchLeader";

    public static readonly IReadOnlyList<string> AllRoles = new[]
    {
        Administrator,
        Developer,
        Guest,
        Member,
        ChurchLeader
    };
} 