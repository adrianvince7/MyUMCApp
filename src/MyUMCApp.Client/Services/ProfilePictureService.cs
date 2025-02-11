using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace MyUMCApp.Client.Services;

public interface IProfilePictureService
{
    Task<string?> UploadProfilePictureAsync(Stream fileStream, string fileName);
}

public class ProfilePictureService : IProfilePictureService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ProfilePictureService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string?> UploadProfilePictureAsync(Stream fileStream, string fileName)
    {
        try
        {
            var apiUrl = _configuration["API:BaseUrl"] + "/api/profile/upload-picture";
            
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            
            content.Add(fileContent, "file", fileName);
            
            var response = await _httpClient.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UploadResult>();
                return result?.Url;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error uploading profile picture: {ex}");
            return null;
        }
    }
}

public class UploadResult
{
    public string Url { get; set; } = string.Empty;
} 