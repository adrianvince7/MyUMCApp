using System.Text.Json.Serialization;

namespace MyUMCApp.Shared.Models;

public class ImageMetadata
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } = string.Empty;
    public bool HasAlpha { get; set; }
    public string ColorSpace { get; set; } = string.Empty;
    public Dictionary<string, string> ExifData { get; set; } = new();
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public Guid UploadedById { get; set; }
    public User? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public CompressionStatistics? CompressionStats { get; set; }
}

public class CompressionStatistics
{
    public long OriginalSize { get; set; }
    public long CompressedSize { get; set; }
    public double CompressionRatio => OriginalSize > 0 ? (double)CompressedSize / OriginalSize : 1;
    public int Quality { get; set; }
    public string Format { get; set; } = string.Empty;
    public TimeSpan ProcessingTime { get; set; }
    public bool IsOptimized { get; set; }
    public Dictionary<string, string> OptimizationSettings { get; set; } = new();
}

public class ImageGallery
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<ImageMetadata> Images { get; set; } = new();
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }
    public GalleryVisibility Visibility { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public List<string> Tags { get; set; } = new();
}

public enum GalleryVisibility
{
    Private,
    Public,
    Shared
}

public class ImageSearchResult
{
    public List<ImageMetadata> Images { get; set; } = new();
    public int TotalCount { get; set; }
    public Dictionary<string, int> TagCounts { get; set; } = new();
    public Dictionary<string, int> FormatCounts { get; set; } = new();
    public Dictionary<string, double> AverageCompressionRatios { get; set; } = new();
} 