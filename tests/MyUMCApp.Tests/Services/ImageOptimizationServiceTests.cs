using System.Drawing;
using FluentAssertions;
using MyUMCApp.Members.API.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using Xunit;

namespace MyUMCApp.Tests.Services;

public class ImageOptimizationServiceTests
{
    private readonly ImageOptimizationService _service;
    private const int MaxDimension = 1200;
    private const int Quality = 80;

    public ImageOptimizationServiceTests()
    {
        _service = new ImageOptimizationService(MaxDimension, Quality);
    }

    [Fact]
    public async Task OptimizeImageAsync_WithLargeImage_ResizesAndCompresses()
    {
        // Arrange
        var largeImageBytes = CreateTestImage(2000, 1500);
        using var inputStream = new MemoryStream(largeImageBytes);

        // Act
        using var outputStream = await _service.OptimizeImageAsync(inputStream);
        using var optimizedImage = await Image.LoadAsync(outputStream);

        // Assert
        optimizedImage.Width.Should().BeLessOrEqualTo(MaxDimension);
        optimizedImage.Height.Should().BeLessOrEqualTo(MaxDimension);
        outputStream.Length.Should().BeLessThan(inputStream.Length);
    }

    [Fact]
    public async Task OptimizeImageAsync_WithSmallImage_MaintainsOriginalSize()
    {
        // Arrange
        var smallImageBytes = CreateTestImage(800, 600);
        using var inputStream = new MemoryStream(smallImageBytes);

        // Act
        using var outputStream = await _service.OptimizeImageAsync(inputStream);
        using var optimizedImage = await Image.LoadAsync(outputStream);

        // Assert
        optimizedImage.Width.Should().Be(800);
        optimizedImage.Height.Should().Be(600);
    }

    [Fact]
    public async Task OptimizeImageAsync_WithInvalidImage_ThrowsException()
    {
        // Arrange
        var invalidImageBytes = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        using var inputStream = new MemoryStream(invalidImageBytes);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidImageFormatException>(
            () => _service.OptimizeImageAsync(inputStream));
    }

    private static byte[] CreateTestImage(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        using var memoryStream = new MemoryStream();
        image.Save(memoryStream, new JpegEncoder());
        return memoryStream.ToArray();
    }
} 