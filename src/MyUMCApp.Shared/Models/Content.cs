namespace MyUMCApp.Shared.Models;

public abstract class Content
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
    public User? Author { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public List<Comment> Comments { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

public class Sermon : Content
{
    public string VideoUrl { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public string TranscriptUrl { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string PreacherName { get; set; } = string.Empty;
    public DateTime SermonDate { get; set; }
    public string Scripture { get; set; } = string.Empty;
    public int Views { get; set; }
    public int Downloads { get; set; }
    public double Rating { get; set; }
    public List<SermonRating> Ratings { get; set; } = new();
}

public class BlogPost : Content
{
    public string Content { get; set; } = string.Empty;
    public string FeaturedImageUrl { get; set; } = string.Empty;
    public int ReadTime { get; set; }
    public int Views { get; set; }
    public List<BlogPostLike> Likes { get; set; } = new();
}

public class Announcement : Content
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AnnouncementPriority Priority { get; set; }
    public bool RequiresAcknowledgement { get; set; }
    public List<AnnouncementAcknowledgement> Acknowledgements { get; set; } = new();
}

public class Comment
{
    public Guid Id { get; set; }
    public Guid ContentId { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public List<Comment> Replies { get; set; } = new();
}

public class SermonRating
{
    public Guid Id { get; set; }
    public Guid SermonId { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public int Rating { get; set; }
    public string? Review { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BlogPostLike
{
    public Guid Id { get; set; }
    public Guid BlogPostId { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AnnouncementAcknowledgement
{
    public Guid Id { get; set; }
    public Guid AnnouncementId { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public DateTime AcknowledgedAt { get; set; }
}

public enum AnnouncementPriority
{
    Low,
    Medium,
    High,
    Urgent
} 