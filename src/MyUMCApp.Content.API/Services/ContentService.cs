using Microsoft.Extensions.Logging;
using MyUMCApp.Shared.Data;
using MyUMCApp.Shared.Models;

namespace MyUMCApp.Content.API.Services;

public interface IContentService
{
    Task<Sermon> CreateSermonAsync(Sermon sermon);
    Task<BlogPost> CreateBlogPostAsync(BlogPost blogPost);
    Task<Announcement> CreateAnnouncementAsync(Announcement announcement);
    Task<Comment> AddCommentAsync(Comment comment);
    Task<SermonRating> RateSermonAsync(SermonRating rating);
    Task<BlogPostLike> LikeBlogPostAsync(BlogPostLike like);
    Task<AnnouncementAcknowledgement> AcknowledgeAnnouncementAsync(AnnouncementAcknowledgement acknowledgement);
    Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync();
    Task<IEnumerable<Sermon>> GetLatestSermonsAsync(int count);
    Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync(int count);
}

public class ContentService : IContentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ContentService> _logger;

    public ContentService(IUnitOfWork unitOfWork, ILogger<ContentService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Sermon> CreateSermonAsync(Sermon sermon)
    {
        try
        {
            sermon.CreatedAt = DateTime.UtcNow;
            sermon.UpdatedAt = DateTime.UtcNow;
            
            var createdSermon = await _unitOfWork.Repository<Sermon>().AddAsync(sermon);
            _logger.LogInformation("Created new sermon with ID: {SermonId}", createdSermon.Id);
            
            return createdSermon;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sermon");
            throw;
        }
    }

    public async Task<BlogPost> CreateBlogPostAsync(BlogPost blogPost)
    {
        try
        {
            blogPost.CreatedAt = DateTime.UtcNow;
            blogPost.UpdatedAt = DateTime.UtcNow;
            
            var createdPost = await _unitOfWork.Repository<BlogPost>().AddAsync(blogPost);
            _logger.LogInformation("Created new blog post with ID: {BlogPostId}", createdPost.Id);
            
            return createdPost;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blog post");
            throw;
        }
    }

    public async Task<Announcement> CreateAnnouncementAsync(Announcement announcement)
    {
        try
        {
            announcement.CreatedAt = DateTime.UtcNow;
            announcement.UpdatedAt = DateTime.UtcNow;
            
            var createdAnnouncement = await _unitOfWork.Repository<Announcement>().AddAsync(announcement);
            _logger.LogInformation("Created new announcement with ID: {AnnouncementId}", createdAnnouncement.Id);
            
            return createdAnnouncement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating announcement");
            throw;
        }
    }

    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        try
        {
            comment.CreatedAt = DateTime.UtcNow;
            
            var createdComment = await _unitOfWork.Repository<Comment>().AddAsync(comment);
            _logger.LogInformation("Added comment with ID: {CommentId} to content: {ContentId}", 
                createdComment.Id, comment.ContentId);
            
            return createdComment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to content: {ContentId}", comment.ContentId);
            throw;
        }
    }

    public async Task<SermonRating> RateSermonAsync(SermonRating rating)
    {
        try
        {
            var sermon = await _unitOfWork.Repository<Sermon>().GetByIdAsync(rating.SermonId);
            if (sermon == null)
            {
                throw new KeyNotFoundException($"Sermon with ID {rating.SermonId} not found");
            }

            rating.CreatedAt = DateTime.UtcNow;
            var createdRating = await _unitOfWork.Repository<SermonRating>().AddAsync(rating);

            // Update sermon's average rating
            sermon.Rating = sermon.Ratings.Any() 
                ? (sermon.Rating * sermon.Ratings.Count + rating.Rating) / (sermon.Ratings.Count + 1)
                : rating.Rating;
            sermon.Ratings.Add(createdRating);
            
            await _unitOfWork.Repository<Sermon>().UpdateAsync(sermon);
            _logger.LogInformation("Added rating with ID: {RatingId} to sermon: {SermonId}", 
                createdRating.Id, rating.SermonId);
            
            return createdRating;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rating sermon: {SermonId}", rating.SermonId);
            throw;
        }
    }

    public async Task<BlogPostLike> LikeBlogPostAsync(BlogPostLike like)
    {
        try
        {
            var blogPost = await _unitOfWork.Repository<BlogPost>().GetByIdAsync(like.BlogPostId);
            if (blogPost == null)
            {
                throw new KeyNotFoundException($"Blog post with ID {like.BlogPostId} not found");
            }

            // Check if user already liked the post
            if (blogPost.Likes.Any(l => l.UserId == like.UserId))
            {
                throw new InvalidOperationException($"User {like.UserId} has already liked this post");
            }

            like.CreatedAt = DateTime.UtcNow;
            var createdLike = await _unitOfWork.Repository<BlogPostLike>().AddAsync(like);

            blogPost.Likes++;
            await _unitOfWork.Repository<BlogPost>().UpdateAsync(blogPost);
            
            _logger.LogInformation("Added like with ID: {LikeId} to blog post: {BlogPostId}", 
                createdLike.Id, like.BlogPostId);
            
            return createdLike;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking blog post: {BlogPostId}", like.BlogPostId);
            throw;
        }
    }

    public async Task<AnnouncementAcknowledgement> AcknowledgeAnnouncementAsync(AnnouncementAcknowledgement acknowledgement)
    {
        try
        {
            var announcement = await _unitOfWork.Repository<Announcement>().GetByIdAsync(acknowledgement.AnnouncementId);
            if (announcement == null)
            {
                throw new KeyNotFoundException($"Announcement with ID {acknowledgement.AnnouncementId} not found");
            }

            if (!announcement.RequiresAcknowledgement)
            {
                throw new InvalidOperationException("This announcement does not require acknowledgement");
            }

            // Check if user already acknowledged
            if (announcement.Acknowledgements.Any(a => a.UserId == acknowledgement.UserId))
            {
                throw new InvalidOperationException($"User {acknowledgement.UserId} has already acknowledged this announcement");
            }

            acknowledgement.AcknowledgedAt = DateTime.UtcNow;
            var createdAck = await _unitOfWork.Repository<AnnouncementAcknowledgement>().AddAsync(acknowledgement);

            announcement.Acknowledgements.Add(createdAck);
            await _unitOfWork.Repository<Announcement>().UpdateAsync(announcement);
            
            _logger.LogInformation("Added acknowledgement with ID: {AckId} to announcement: {AnnouncementId}", 
                createdAck.Id, acknowledgement.AnnouncementId);
            
            return createdAck;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging announcement: {AnnouncementId}", acknowledgement.AnnouncementId);
            throw;
        }
    }

    public async Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var announcements = await _unitOfWork.Repository<Announcement>()
                .FindAsync(a => a.IsPublished && 
                               a.StartDate <= now && 
                               a.EndDate >= now);
            
            return announcements.OrderByDescending(a => a.Priority)
                              .ThenByDescending(a => a.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active announcements");
            throw;
        }
    }

    public async Task<IEnumerable<Sermon>> GetLatestSermonsAsync(int count)
    {
        try
        {
            var sermons = await _unitOfWork.Repository<Sermon>()
                .FindAsync(s => s.IsPublished);
            
            return sermons.OrderByDescending(s => s.SermonDate)
                         .Take(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest sermons");
            throw;
        }
    }

    public async Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync(int count)
    {
        try
        {
            var posts = await _unitOfWork.Repository<BlogPost>()
                .FindAsync(p => p.IsPublished);
            
            return posts.OrderByDescending(p => p.Views + (p.Likes * 2)) // Weight likes more than views
                       .Take(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving popular blog posts");
            throw;
        }
    }
} 