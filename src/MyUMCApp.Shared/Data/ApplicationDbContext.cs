using Microsoft.EntityFrameworkCore;
using MyUMCApp.Shared.Models;

namespace MyUMCApp.Shared.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<GivingRecord> GivingRecords => Set<GivingRecord>();
    public DbSet<MembershipHistory> MembershipHistories => Set<MembershipHistory>();
    public DbSet<Sermon> Sermons => Set<Sermon>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<SermonRating> SermonRatings => Set<SermonRating>();
    public DbSet<BlogPostLike> BlogPostLikes => Set<BlogPostLike>();
    public DbSet<AnnouncementAcknowledgement> AnnouncementAcknowledgements => Set<AnnouncementAcknowledgement>();
    public DbSet<ChurchEvent> Events => Set<ChurchEvent>();
    public DbSet<RecurrencePattern> RecurrencePatterns => Set<RecurrencePattern>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<EventReminder> EventReminders => Set<EventReminder>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.PreferredLanguage).HasMaxLength(10);
        });

        // Member configuration
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Member>(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Organization).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.EmergencyContact).HasMaxLength(100);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
        });

        // Content configuration
        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Event configuration
        modelBuilder.Entity<ChurchEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Location).HasMaxLength(500);
            entity.Property(e => e.VirtualMeetingUrl).HasMaxLength(500);
            entity.HasOne(e => e.Organizer)
                .WithMany()
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Store configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.SubCategory).HasMaxLength(100);
            entity.HasIndex(e => e.SKU).IsUnique();
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);
            entity.Property(e => e.BillingAddress).HasMaxLength(500);
            entity.Property(e => e.PaymentReference).HasMaxLength(100);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
        });

        // Cart configuration
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
} 