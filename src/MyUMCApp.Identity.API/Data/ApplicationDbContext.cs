using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyUMCApp.Identity.API.Models;

namespace MyUMCApp.Identity.API.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Custom configurations
        builder.Entity<ApplicationUser>()
            .Property(u => u.FirstName)
            .HasMaxLength(100);
            
        builder.Entity<ApplicationUser>()
            .Property(u => u.LastName)
            .HasMaxLength(100);
    }
} 