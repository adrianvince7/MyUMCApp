using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyUMCApp.API.Models;
using MyUMCApp.API.Services;
using System.Security.Claims;

namespace MyUMCApp.API.Controllers;

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
    [Authorize(Roles = "Administrator,ChurchLeader")]
    public async Task<ActionResult<Sermon>> CreateSermon(Sermon sermon)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            sermon.AuthorId = currentUserId;
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
    [AllowAnonymous]
    public async Task<ActionResult<Sermon>> GetSermon(Guid id)
    {
        try
        {
            var sermons = await _contentService.GetLatestSermonsAsync(100); // Get more to find specific one
            var sermon = sermons.FirstOrDefault(s => s.Id == id);
            if (sermon == null)
            {
                return NotFound($"Sermon with ID {id} not found");
            }

            return Ok(sermon);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sermon with ID: {SermonId}", id);
            return StatusCode(500, "An error occurred while retrieving the sermon");
        }
    }

    [HttpGet("sermons/latest")]
    [AllowAnonymous]
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
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            rating.UserId = currentUserId;
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
    [Authorize(Roles = "Administrator,ChurchLeader")]
    public async Task<ActionResult<BlogPost>> CreateBlogPost(BlogPost blogPost)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            blogPost.AuthorId = currentUserId;
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
    [AllowAnonymous]
    public async Task<ActionResult<BlogPost>> GetBlogPost(Guid id)
    {
        try
        {
            var posts = await _contentService.GetPopularBlogPostsAsync(100); // Get more to find specific one
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
    [AllowAnonymous]
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
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            like.UserId = currentUserId;
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
    [Authorize(Roles = "Administrator,ChurchLeader")]
    public async Task<ActionResult<Announcement>> CreateAnnouncement(Announcement announcement)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            announcement.AuthorId = currentUserId;
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
    [AllowAnonymous]
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
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            acknowledgement.UserId = currentUserId;
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
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            comment.UserId = currentUserId;
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