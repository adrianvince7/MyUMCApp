using MyUMCApp.API.Models;

namespace MyUMCApp.API.Services;

public interface IContentService
{
    // Sermon management
    Task<Sermon> CreateSermonAsync(Sermon sermon);
    Task<IEnumerable<Sermon>> GetLatestSermonsAsync(int count = 10);
    Task<SermonRating> RateSermonAsync(SermonRating rating);

    // Blog post management
    Task<BlogPost> CreateBlogPostAsync(BlogPost blogPost);
    Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync(int count = 10);
    Task<BlogPostLike> LikeBlogPostAsync(BlogPostLike like);

    // Announcement management
    Task<Announcement> CreateAnnouncementAsync(Announcement announcement);
    Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync();
    Task<AnnouncementAcknowledgement> AcknowledgeAnnouncementAsync(AnnouncementAcknowledgement acknowledgement);

    // Comment management
    Task<Comment> AddCommentAsync(Comment comment);
}