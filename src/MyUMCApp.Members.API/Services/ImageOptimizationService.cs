using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace MyUMCApp.Members.API.Services;

public interface IImageOptimizationService
{
    Task<Stream> OptimizeImageAsync(Stream inputStream);
}

public class ImageOptimizationService : IImageOptimizationService
{
    private readonly int _maxDimension;
    private readonly int _quality;

    public ImageOptimizationService(int maxDimension = 1200, int quality = 80)
    {
        _maxDimension = maxDimension;
        _quality = quality;
    }

    public async Task<Stream> OptimizeImageAsync(Stream inputStream)
    {
        using var image = await Image.LoadAsync(inputStream);
        
        // Calculate new dimensions while maintaining aspect ratio
        var (newWidth, newHeight) = CalculateNewDimensions(image.Width, image.Height);
        
        // Only resize if the image is larger than max dimensions
        if (newWidth != image.Width || newHeight != image.Height)
        {
            image.Mutate(x => x.Resize(newWidth, newHeight));
        }

        // Create output stream and save the optimized image
        var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, GetOptimizedEncoder());
        outputStream.Position = 0;
        
        return outputStream;
    }

    private (int width, int height) CalculateNewDimensions(int currentWidth, int currentHeight)
    {
        if (currentWidth <= _maxDimension && currentHeight <= _maxDimension)
        {
            return (currentWidth, currentHeight);
        }

        var ratio = Math.Min((double)_maxDimension / currentWidth, (double)_maxDimension / currentHeight);
        return ((int)(currentWidth * ratio), (int)(currentHeight * ratio));
    }

    private IImageEncoder GetOptimizedEncoder()
    {
        return new JpegEncoder
        {
            Quality = _quality,
            ColorType = JpegColorType.YCbCr,
            SkipMetadata = true
        };
    }
} 