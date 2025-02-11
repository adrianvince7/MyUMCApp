using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace MyUMCApp.Members.API.Services;

public interface IProfileStorageService
{
    Task<string> UploadProfilePictureAsync(Stream fileStream, string fileName);
}

public class ProfileStorageService : IProfileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly string _bucketName;
    private readonly string _cdnDomain;

    public ProfileStorageService(
        IAmazonS3 s3Client,
        IConfiguration configuration)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _bucketName = _configuration["AWS:S3:ProfilePicturesBucket"] 
            ?? throw new InvalidOperationException("S3 bucket name not configured");
        _cdnDomain = _configuration["AWS:CloudFront:Domain"] 
            ?? throw new InvalidOperationException("CloudFront domain not configured");
    }

    public async Task<string> UploadProfilePictureAsync(Stream fileStream, string fileName)
    {
        try
        {
            var key = $"profiles/{fileName}";
            
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = GetContentType(fileName),
                CannedACL = S3CannedACL.Private // Ensure the file is private, accessed through CloudFront
            };

            await _s3Client.PutObjectAsync(putRequest);

            // Return the CloudFront URL
            return $"https://{_cdnDomain}/{key}";
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to upload profile picture to S3", ex);
        }
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }
} 