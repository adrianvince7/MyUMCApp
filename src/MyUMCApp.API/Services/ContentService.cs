using Microsoft.EntityFrameworkCore;
using MyUMCApp.API.Data;
using MyUMCApp.API.Models;

namespace MyUMCApp.API.Services;

public class ContentService : IContentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ContentService> _logger;

    public ContentService(ApplicationDbContext context, ILogger<ContentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Sermon> CreateSermonAsync(Sermon sermon)
    {
        sermon.Id = Guid.NewGuid();
        sermon.CreatedAt = DateTime.UtcNow;
        sermon.UpdatedAt = DateTime.UtcNow;
        sermon.IsPublished = true;
        sermon.PublishedAt = DateTime.UtcNow;

        _context.Sermons.Add(sermon);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new sermon with ID: {SermonId}", sermon.Id);
        return sermon;
    }

    public async Task<IEnumerable<Sermon>> GetLatestSermonsAsync(int count = 10)
    {
        return await _context.Sermons
            .Include(s => s.Author)
            .Include(s => s.Ratings)
            .Where(s => s.IsPublished)
            .OrderByDescending(s => s.SermonDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<SermonRating> RateSermonAsync(SermonRating rating)
    {
        var sermon = await _context.Sermons.FindAsync(rating.SermonId);
        if (sermon == null)
        {
            throw new KeyNotFoundException($"Sermon with ID {rating.SermonId} not found");
        }

        // Check if user has already rated this sermon
        var existingRating = await _context.SermonRatings
            .FirstOrDefaultAsync(r => r.SermonId == rating.SermonId && r.UserId == rating.UserId);

        if (existingRating != null)
        {
            // Update existing rating
            existingRating.Rating = rating.Rating;
            existingRating.Review = rating.Review;
            existingRating.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            // Create new rating
            rating.Id = Guid.NewGuid();
            rating.CreatedAt = DateTime.UtcNow;
            _context.SermonRatings.Add(rating);
        }

        await _context.SaveChangesAsync();

        // Update sermon average rating
        await UpdateSermonRatingAsync(rating.SermonId);

        _logger.LogInformation("Rated sermon {SermonId} with {Rating} stars", rating.SermonId, rating.Rating);
        return existingRating ?? rating;
    }

    public async Task<BlogPost> CreateBlogPostAsync(BlogPost blogPost)
    {
        blogPost.Id = Guid.NewGuid();
        blogPost.CreatedAt = DateTime.UtcNow;
        blogPost.UpdatedAt = DateTime.UtcNow;
        blogPost.IsPublished = true;
        blogPost.PublishedAt = DateTime.UtcNow;

        _context.BlogPosts.Add(blogPost);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new blog post with ID: {BlogPostId}", blogPost.Id);
        return blogPost;
    }

    public async Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync(int count = 10)
    {
        return await _context.BlogPosts
            .Include(b => b.Author)
            .Include(b => b.Likes)
            .Where(b => b.IsPublished)
            .OrderByDescending(b => b.Views)
            .ThenByDescending(b => b.Likes.Count)
            .Take(count)
            .ToListAsync();
    }

    public async Task<BlogPostLike> LikeBlogPostAsync(BlogPostLike like)
    {
        var blogPost = await _context.BlogPosts.FindAsync(like.BlogPostId);
        if (blogPost == null)
        {
            throw new KeyNotFoundException($"Blog post with ID {like.BlogPostId} not found");
        }

        // Check if user has already liked this post
        var existingLike = await _context.BlogPostLikes
            .FirstOrDefaultAsync(l => l.BlogPostId == like.BlogPostId && l.UserId == like.UserId);

        if (existingLike != null)
        {
            throw new InvalidOperationException("User has already liked this blog post");
        }

        like.Id = Guid.NewGuid();
        like.CreatedAt = DateTime.UtcNow;

        _context.BlogPostLikes.Add(like);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} liked blog post {BlogPostId}", like.UserId, like.BlogPostId);
        return like;
    }

    public async Task<Announcement> CreateAnnouncementAsync(Announcement announcement)
    {
        announcement.Id = Guid.NewGuid();
        announcement.CreatedAt = DateTime.UtcNow;
        announcement.UpdatedAt = DateTime.UtcNow;
        announcement.IsPublished = true;
        announcement.PublishedAt = DateTime.UtcNow;

        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new announcement with ID: {AnnouncementId}", announcement.Id);
        return announcement;
    }

    public async Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Announcements
            .Include(a => a.Author)
            .Where(a => a.IsPublished && 
                       a.StartDate <= now && 
                       a.EndDate >= now)
            .OrderByDescending(a => a.Priority)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<AnnouncementAcknowledgement> AcknowledgeAnnouncementAsync(AnnouncementAcknowledgement acknowledgement)
    {
        var announcement = await _context.Announcements.FindAsync(acknowledgement.AnnouncementId);
        if (announcement == null)
        {
            throw new KeyNotFoundException($"Announcement with ID {acknowledgement.AnnouncementId} not found");
        }

        if (!announcement.RequiresAcknowledgement)
        {
            throw new InvalidOperationException("This announcement does not require acknowledgement");
        }

        // Check if user has already acknowledged this announcement
        var existingAck = await _context.AnnouncementAcknowledgements
            .FirstOrDefaultAsync(a => a.AnnouncementId == acknowledgement.AnnouncementId && 
                                     a.UserId == acknowledgement.UserId);

        if (existingAck != null)
        {
            throw new InvalidOperationException("User has already acknowledged this announcement");
        }

        acknowledgement.Id = Guid.NewGuid();
        acknowledgement.AcknowledgedAt = DateTime.UtcNow;

        _context.AnnouncementAcknowledgements.Add(acknowledgement);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} acknowledged announcement {AnnouncementId}", 
            acknowledgement.UserId, acknowledgement.AnnouncementId);
        return acknowledgement;
    }

    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        comment.Id = Guid.NewGuid();
        comment.CreatedAt = DateTime.UtcNow;

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added comment to content {ContentId} by user {UserId}", 
            comment.ContentId, comment.UserId);
        return comment;
    }

    private async Task UpdateSermonRatingAsync(Guid sermonId)
    {
        var ratings = await _context.SermonRatings
            .Where(r => r.SermonId == sermonId)
            .Select(r => r.Rating)
            .ToListAsync();

        if (ratings.Any())
        {
            var sermon = await _context.Sermons.FindAsync(sermonId);
            if (sermon != null)
            {
                sermon.Rating = ratings.Average();
                await _context.SaveChangesAsync();
            }
        }
    }
}