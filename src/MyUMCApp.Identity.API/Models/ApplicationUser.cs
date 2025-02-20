using Microsoft.AspNetCore.Identity;

namespace MyUMCApp.Identity.API.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Organization { get; set; }
    public string ChurchRole { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; }
} 