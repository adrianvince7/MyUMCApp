using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyUMCApp.Content.API.Services;
using MyUMCApp.Shared.Models;

namespace MyUMCApp.Content.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ContentController : ControllerBase
{
    private readonly IContentService _contentService;
    private readonly ILogger<ContentController> _logger;

    public ContentController(IContentService contentService, ILogger<ContentController> logger)
    {
        _contentService = contentService;
        _logger = logger;
    }

    [HttpPost("sermons")]
    public async Task<ActionResult<Sermon>> CreateSermon(Sermon sermon)
    {
        try
        {
            var createdSermon = await _contentService.CreateSermonAsync(sermon);
            return CreatedAtAction(nameof(GetSermon), new { id = createdSermon.Id }, createdSermon);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sermon");
            return StatusCode(500, "An error occurred while creating the sermon");
        }
    }

    [HttpGet("sermons/{id:guid}")]
    public async Task<ActionResult<Sermon>> GetSermon(Guid id)
    {
        try
        {
            var sermon = await _contentService.GetLatestSermonsAsync(1);
            var result = sermon.FirstOrDefault(s => s.Id == id);
            if (result == null)
            {
                return NotFound($"Sermon with ID {id} not found");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sermon with ID: {SermonId}", id);
            return StatusCode(500, "An error occurred while retrieving the sermon");
        }
    }

    [HttpGet("sermons/latest")]
    public async Task<ActionResult<IEnumerable<Sermon>>> GetLatestSermons([FromQuery] int count = 5)
    {
        try
        {
            var sermons = await _contentService.GetLatestSermonsAsync(count);
            return Ok(sermons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest sermons");
            return StatusCode(500, "An error occurred while retrieving the latest sermons");
        }
    }

    [HttpPost("sermons/{id:guid}/ratings")]
    public async Task<ActionResult<SermonRating>> RateSermon(Guid id, SermonRating rating)
    {
        if (id != rating.SermonId)
        {
            return BadRequest("Sermon ID mismatch");
        }

        try
        {
            var result = await _contentService.RateSermonAsync(rating);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rating sermon with ID: {SermonId}", id);
            return StatusCode(500, "An error occurred while rating the sermon");
        }
    }

    [HttpPost("blog-posts")]
    public async Task<ActionResult<BlogPost>> CreateBlogPost(BlogPost blogPost)
    {
        try
        {
            var createdPost = await _contentService.CreateBlogPostAsync(blogPost);
            return CreatedAtAction(nameof(GetBlogPost), new { id = createdPost.Id }, createdPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blog post");
            return StatusCode(500, "An error occurred while creating the blog post");
        }
    }

    [HttpGet("blog-posts/{id:guid}")]
    public async Task<ActionResult<BlogPost>> GetBlogPost(Guid id)
    {
        try
        {
            var posts = await _contentService.GetPopularBlogPostsAsync(int.MaxValue);
            var post = posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                return NotFound($"Blog post with ID {id} not found");
            }

            return Ok(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blog post with ID: {BlogPostId}", id);
            return StatusCode(500, "An error occurred while retrieving the blog post");
        }
    }

    [HttpGet("blog-posts/popular")]
    public async Task<ActionResult<IEnumerable<BlogPost>>> GetPopularBlogPosts([FromQuery] int count = 5)
    {
        try
        {
            var posts = await _contentService.GetPopularBlogPostsAsync(count);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving popular blog posts");
            return StatusCode(500, "An error occurred while retrieving popular blog posts");
        }
    }

    [HttpPost("blog-posts/{id:guid}/likes")]
    public async Task<ActionResult<BlogPostLike>> LikeBlogPost(Guid id, BlogPostLike like)
    {
        if (id != like.BlogPostId)
        {
            return BadRequest("Blog post ID mismatch");
        }

        try
        {
            var result = await _contentService.LikeBlogPostAsync(like);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking blog post with ID: {BlogPostId}", id);
            return StatusCode(500, "An error occurred while liking the blog post");
        }
    }

    [HttpPost("announcements")]
    public async Task<ActionResult<Announcement>> CreateAnnouncement(Announcement announcement)
    {
        try
        {
            var createdAnnouncement = await _contentService.CreateAnnouncementAsync(announcement);
            return CreatedAtAction(nameof(GetActiveAnnouncements), null, createdAnnouncement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating announcement");
            return StatusCode(500, "An error occurred while creating the announcement");
        }
    }

    [HttpGet("announcements/active")]
    public async Task<ActionResult<IEnumerable<Announcement>>> GetActiveAnnouncements()
    {
        try
        {
            var announcements = await _contentService.GetActiveAnnouncementsAsync();
            return Ok(announcements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active announcements");
            return StatusCode(500, "An error occurred while retrieving active announcements");
        }
    }

    [HttpPost("announcements/{id:guid}/acknowledgements")]
    public async Task<ActionResult<AnnouncementAcknowledgement>> AcknowledgeAnnouncement(Guid id, AnnouncementAcknowledgement acknowledgement)
    {
        if (id != acknowledgement.AnnouncementId)
        {
            return BadRequest("Announcement ID mismatch");
        }

        try
        {
            var result = await _contentService.AcknowledgeAnnouncementAsync(acknowledgement);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging announcement with ID: {AnnouncementId}", id);
            return StatusCode(500, "An error occurred while acknowledging the announcement");
        }
    }

    [HttpPost("{contentId:guid}/comments")]
    public async Task<ActionResult<Comment>> AddComment(Guid contentId, Comment comment)
    {
        if (contentId != comment.ContentId)
        {
            return BadRequest("Content ID mismatch");
        }

        try
        {
            var result = await _contentService.AddCommentAsync(comment);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to content with ID: {ContentId}", contentId);
            return StatusCode(500, "An error occurred while adding the comment");
        }
    }
} 