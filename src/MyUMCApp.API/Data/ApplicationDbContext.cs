using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyUMCApp.API.Models;

namespace MyUMCApp.API.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Member Management
    public DbSet<Member> Members { get; set; }
    public DbSet<GivingRecord> GivingRecords { get; set; }
    public DbSet<MembershipHistory> MembershipHistories { get; set; }

    // Content Management
    public DbSet<Sermon> Sermons { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<SermonRating> SermonRatings { get; set; }
    public DbSet<BlogPostLike> BlogPostLikes { get; set; }
    public DbSet<AnnouncementAcknowledgement> AnnouncementAcknowledgements { get; set; }

    // Store Management
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    // Event Management
    public DbSet<ChurchEvent> ChurchEvents { get; set; }
    public DbSet<RecurrencePattern> RecurrencePatterns { get; set; }
    public DbSet<EventRegistration> EventRegistrations { get; set; }
    public DbSet<EventReminder> EventReminders { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Custom configurations for ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(100);
            entity.Property(u => u.LastName).HasMaxLength(100);
            entity.Property(u => u.Organization).HasMaxLength(200);
            entity.Property(u => u.ChurchRole).HasMaxLength(100);
        });

        // Member configurations
        builder.Entity<Member>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Organization).HasMaxLength(200);
            entity.Property(m => m.Address).HasMaxLength(500);
            entity.Property(m => m.EmergencyContact).HasMaxLength(100);
            entity.Property(m => m.EmergencyContactPhone).HasMaxLength(20);
            
            entity.HasOne(m => m.User)
                  .WithMany()
                  .HasForeignKey(m => m.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(m => m.GivingHistory)
                  .WithOne(g => g.Member)
                  .HasForeignKey(g => g.MemberId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(m => m.MembershipHistory)
                  .WithOne(h => h.Member)
                  .HasForeignKey(h => h.MemberId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Content configurations
        builder.Entity<Content>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Title).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(1000);
            
            entity.HasOne(c => c.Author)
                  .WithMany()
                  .HasForeignKey(c => c.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasMany(c => c.Comments)
                  .WithOne()
                  .HasForeignKey(c => c.ContentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Sermon>(entity =>
        {
            entity.Property(s => s.VideoUrl).HasMaxLength(500);
            entity.Property(s => s.AudioUrl).HasMaxLength(500);
            entity.Property(s => s.TranscriptUrl).HasMaxLength(500);
            entity.Property(s => s.PreacherName).HasMaxLength(100);
            entity.Property(s => s.Scripture).HasMaxLength(200);
        });

        builder.Entity<BlogPost>(entity =>
        {
            entity.Property(b => b.FeaturedImageUrl).HasMaxLength(500);
        });

        // Store configurations
        builder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.SKU).HasMaxLength(50).IsRequired();
            entity.Property(p => p.Category).HasMaxLength(100);
            entity.Property(p => p.SubCategory).HasMaxLength(100);
            entity.Property(p => p.Price).HasColumnType("decimal(10,2)");
            
            entity.HasMany(p => p.Variants)
                  .WithOne(v => v.Product)
                  .HasForeignKey(v => v.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductVariant>(entity =>
        {
            entity.Property(v => v.Name).HasMaxLength(200);
            entity.Property(v => v.SKU).HasMaxLength(50);
            entity.Property(v => v.Price).HasColumnType("decimal(10,2)");
        });

        builder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            entity.Property(o => o.SubTotal).HasColumnType("decimal(10,2)");
            entity.Property(o => o.ShippingCost).HasColumnType("decimal(10,2)");
            entity.Property(o => o.Tax).HasColumnType("decimal(10,2)");
            entity.Property(o => o.Total).HasColumnType("decimal(10,2)");
            entity.Property(o => o.ShippingAddress).HasMaxLength(500);
            entity.Property(o => o.BillingAddress).HasMaxLength(500);
            
            entity.HasOne(o => o.User)
                  .WithMany()
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasMany(o => o.Items)
                  .WithOne(i => i.Order)
                  .HasForeignKey(i => i.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");
            entity.Property(i => i.Total).HasColumnType("decimal(10,2)");
        });

        // Event configurations
        builder.Entity<ChurchEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Location).HasMaxLength(300);
            entity.Property(e => e.VirtualMeetingUrl).HasMaxLength(500);
            entity.Property(e => e.RegistrationFee).HasColumnType("decimal(10,2)");
            
            entity.HasOne(e => e.Organizer)
                  .WithMany()
                  .HasForeignKey(e => e.OrganizerId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasMany(e => e.Registrations)
                  .WithOne(r => r.Event)
                  .HasForeignKey(r => r.EventId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}