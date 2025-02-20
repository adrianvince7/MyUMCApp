using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyUMCApp.Content.API.Services;
using MyUMCApp.Shared.Data;
using MyUMCApp.Shared.Models;
using Xunit;

namespace MyUMCApp.Tests.Services;

public class ContentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ContentService>> _loggerMock;
    private readonly ContentService _service;

    public ContentServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ContentService>>();
        _service = new ContentService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateSermonAsync_WithValidData_ReturnsSermon()
    {
        // Arrange
        var sermon = new Sermon
        {
            Title = "Test Sermon",
            Description = "Test Description",
            AuthorId = Guid.NewGuid(),
            PreacherName = "John Doe",
            Scripture = "John 3:16",
            SermonDate = DateTime.UtcNow
        };

        var sermonRepoMock = new Mock<IRepository<Sermon>>();
        sermonRepoMock
            .Setup(x => x.AddAsync(It.IsAny<Sermon>()))
            .ReturnsAsync(sermon);

        _unitOfWorkMock
            .Setup(x => x.Repository<Sermon>())
            .Returns(sermonRepoMock.Object);

        // Act
        var result = await _service.CreateSermonAsync(sermon);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(sermon);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task CreateBlogPostAsync_WithValidData_ReturnsBlogPost()
    {
        // Arrange
        var blogPost = new BlogPost
        {
            Title = "Test Blog Post",
            Description = "Test Description",
            AuthorId = Guid.NewGuid(),
            Content = "Test Content",
            ReadTime = 5
        };

        var blogPostRepoMock = new Mock<IRepository<BlogPost>>();
        blogPostRepoMock
            .Setup(x => x.AddAsync(It.IsAny<BlogPost>()))
            .ReturnsAsync(blogPost);

        _unitOfWorkMock
            .Setup(x => x.Repository<BlogPost>())
            .Returns(blogPostRepoMock.Object);

        // Act
        var result = await _service.CreateBlogPostAsync(blogPost);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(blogPost);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task CreateAnnouncementAsync_WithValidData_ReturnsAnnouncement()
    {
        // Arrange
        var announcement = new Announcement
        {
            Title = "Test Announcement",
            Description = "Test Description",
            AuthorId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Priority = AnnouncementPriority.High
        };

        var announcementRepoMock = new Mock<IRepository<Announcement>>();
        announcementRepoMock
            .Setup(x => x.AddAsync(It.IsAny<Announcement>()))
            .ReturnsAsync(announcement);

        _unitOfWorkMock
            .Setup(x => x.Repository<Announcement>())
            .Returns(announcementRepoMock.Object);

        // Act
        var result = await _service.CreateAnnouncementAsync(announcement);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(announcement);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task AddCommentAsync_WithValidData_AddsComment()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var comment = new Comment
        {
            ContentId = contentId,
            UserId = userId,
            Text = "Test Comment"
        };

        var commentRepoMock = new Mock<IRepository<Comment>>();
        commentRepoMock
            .Setup(x => x.AddAsync(It.IsAny<Comment>()))
            .ReturnsAsync(comment);

        _unitOfWorkMock
            .Setup(x => x.Repository<Comment>())
            .Returns(commentRepoMock.Object);

        // Act
        var result = await _service.AddCommentAsync(comment);

        // Assert
        result.Should().NotBeNull();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        commentRepoMock.Verify(x => x.AddAsync(It.Is<Comment>(c => 
            c.ContentId == contentId && 
            c.UserId == userId)), Times.Once);
    }

    [Fact]
    public async Task RateSermonAsync_WithValidData_AddsRating()
    {
        // Arrange
        var sermonId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var rating = new SermonRating
        {
            SermonId = sermonId,
            UserId = userId,
            Rating = 5,
            Review = "Great sermon!"
        };

        var sermon = new Sermon
        {
            Id = sermonId,
            Ratings = new List<SermonRating>()
        };

        var sermonRepoMock = new Mock<IRepository<Sermon>>();
        sermonRepoMock
            .Setup(x => x.GetByIdAsync(sermonId))
            .ReturnsAsync(sermon);

        var ratingRepoMock = new Mock<IRepository<SermonRating>>();
        ratingRepoMock
            .Setup(x => x.AddAsync(It.IsAny<SermonRating>()))
            .ReturnsAsync(rating);

        _unitOfWorkMock
            .Setup(x => x.Repository<Sermon>())
            .Returns(sermonRepoMock.Object);
        _unitOfWorkMock
            .Setup(x => x.Repository<SermonRating>())
            .Returns(ratingRepoMock.Object);

        // Act
        var result = await _service.RateSermonAsync(rating);

        // Assert
        result.Should().NotBeNull();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        sermonRepoMock.Verify(x => x.UpdateAsync(It.Is<Sermon>(s => 
            s.Id == sermonId)), Times.Once);
    }

    [Fact]
    public async Task LikeBlogPostAsync_WithValidData_AddsLike()
    {
        // Arrange
        var blogPostId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var like = new BlogPostLike
        {
            BlogPostId = blogPostId,
            UserId = userId
        };

        var blogPost = new BlogPost
        {
            Id = blogPostId,
            Likes = new List<BlogPostLike>()
        };

        var blogPostRepoMock = new Mock<IRepository<BlogPost>>();
        blogPostRepoMock
            .Setup(x => x.GetByIdAsync(blogPostId))
            .ReturnsAsync(blogPost);

        var likeRepoMock = new Mock<IRepository<BlogPostLike>>();
        likeRepoMock
            .Setup(x => x.AddAsync(It.IsAny<BlogPostLike>()))
            .ReturnsAsync(like);

        _unitOfWorkMock
            .Setup(x => x.Repository<BlogPost>())
            .Returns(blogPostRepoMock.Object);
        _unitOfWorkMock
            .Setup(x => x.Repository<BlogPostLike>())
            .Returns(likeRepoMock.Object);

        // Act
        var result = await _service.LikeBlogPostAsync(like);

        // Assert
        result.Should().NotBeNull();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        blogPostRepoMock.Verify(x => x.UpdateAsync(It.Is<BlogPost>(b => 
            b.Id == blogPostId)), Times.Once);
    }

    [Fact]
    public async Task AcknowledgeAnnouncementAsync_WithValidData_AddsAcknowledgement()
    {
        // Arrange
        var announcementId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var acknowledgement = new AnnouncementAcknowledgement
        {
            AnnouncementId = announcementId,
            UserId = userId
        };

        var announcement = new Announcement
        {
            Id = announcementId,
            RequiresAcknowledgement = true,
            Acknowledgements = new List<AnnouncementAcknowledgement>()
        };

        var announcementRepoMock = new Mock<IRepository<Announcement>>();
        announcementRepoMock
            .Setup(x => x.GetByIdAsync(announcementId))
            .ReturnsAsync(announcement);

        var ackRepoMock = new Mock<IRepository<AnnouncementAcknowledgement>>();
        ackRepoMock
            .Setup(x => x.AddAsync(It.IsAny<AnnouncementAcknowledgement>()))
            .ReturnsAsync(acknowledgement);

        _unitOfWorkMock
            .Setup(x => x.Repository<Announcement>())
            .Returns(announcementRepoMock.Object);
        _unitOfWorkMock
            .Setup(x => x.Repository<AnnouncementAcknowledgement>())
            .Returns(ackRepoMock.Object);

        // Act
        var result = await _service.AcknowledgeAnnouncementAsync(acknowledgement);

        // Assert
        result.Should().NotBeNull();
        result.AcknowledgedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        announcementRepoMock.Verify(x => x.UpdateAsync(It.Is<Announcement>(a => 
            a.Id == announcementId)), Times.Once);
    }

    [Fact]
    public async Task GetActiveAnnouncementsAsync_ReturnsActiveAnnouncements()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var announcements = new List<Announcement>
        {
            new() { StartDate = now.AddDays(-1), EndDate = now.AddDays(1) },
            new() { StartDate = now.AddDays(-2), EndDate = now.AddDays(2) }
        };

        var announcementRepoMock = new Mock<IRepository<Announcement>>();
        announcementRepoMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Announcement, bool>>>()))
            .ReturnsAsync(announcements);

        _unitOfWorkMock
            .Setup(x => x.Repository<Announcement>())
            .Returns(announcementRepoMock.Object);

        // Act
        var result = await _service.GetActiveAnnouncementsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLatestSermonsAsync_ReturnsLatestSermons()
    {
        // Arrange
        var sermons = new List<Sermon>
        {
            new() { SermonDate = DateTime.UtcNow.AddDays(-1) },
            new() { SermonDate = DateTime.UtcNow.AddDays(-2) }
        };

        var sermonRepoMock = new Mock<IRepository<Sermon>>();
        sermonRepoMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sermon, bool>>>()))
            .ReturnsAsync(sermons);

        _unitOfWorkMock
            .Setup(x => x.Repository<Sermon>())
            .Returns(sermonRepoMock.Object);

        // Act
        var result = await _service.GetLatestSermonsAsync(5);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPopularBlogPostsAsync_ReturnsPopularPosts()
    {
        // Arrange
        var posts = new List<BlogPost>
        {
            new() { Views = 100, Likes = 50 },
            new() { Views = 200, Likes = 75 }
        };

        var blogPostRepoMock = new Mock<IRepository<BlogPost>>();
        blogPostRepoMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<BlogPost, bool>>>()))
            .ReturnsAsync(posts);

        _unitOfWorkMock
            .Setup(x => x.Repository<BlogPost>())
            .Returns(blogPostRepoMock.Object);

        // Act
        var result = await _service.GetPopularBlogPostsAsync(5);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }
} 