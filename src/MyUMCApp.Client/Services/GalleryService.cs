using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using MyUMCApp.Shared.Models;

namespace MyUMCApp.Client.Services;

public interface IGalleryService
{
    Task<ImageGallery> CreateGalleryAsync(ImageGallery gallery);
    Task<ImageGallery?> GetGalleryAsync(Guid id);
    Task<List<ImageGallery>> GetUserGalleriesAsync();
    Task<bool> UpdateGalleryAsync(ImageGallery gallery);
    Task<bool> DeleteGalleryAsync(Guid id);
    Task<List<ImageMetadata>> UploadImagesAsync(Guid galleryId, IEnumerable<FileUploadRequest> files);
    Task<ImageSearchResult> SearchImagesAsync(ImageSearchRequest request);
    Task<ImageMetadata?> GetImageMetadataAsync(Guid id);
    Task<bool> UpdateImageMetadataAsync(Guid id, ImageMetadataUpdate update);
    Task<bool> DeleteImageAsync(Guid id);
}

public class GalleryService : IGalleryService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GalleryService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<ImageGallery> CreateGalleryAsync(ImageGallery gallery)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_configuration["API:BaseUrl"]}/api/galleries", gallery);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ImageGallery>() 
            ?? throw new InvalidOperationException("Failed to create gallery");
    }

    public async Task<ImageGallery?> GetGalleryAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<ImageGallery>($"{_configuration["API:BaseUrl"]}/api/galleries/{id}");
    }

    public async Task<List<ImageGallery>> GetUserGalleriesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ImageGallery>>($"{_configuration["API:BaseUrl"]}/api/galleries")
            ?? new List<ImageGallery>();
    }

    public async Task<bool> UpdateGalleryAsync(ImageGallery gallery)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"{_configuration["API:BaseUrl"]}/api/galleries/{gallery.Id}", gallery);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteGalleryAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"{_configuration["API:BaseUrl"]}/api/galleries/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ImageMetadata>> UploadImagesAsync(Guid galleryId, IEnumerable<FileUploadRequest> files)
    {
        using var content = new MultipartFormDataContent();
        var index = 0;

        foreach (var file in files)
        {
            var streamContent = new StreamContent(file.Stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, $"files[{index}]", file.FileName);

            if (!string.IsNullOrEmpty(file.Description))
            {
                content.Add(new StringContent(file.Description), $"descriptions[{index}]");
            }

            if (file.Tags?.Any() == true)
            {
                content.Add(new StringContent(string.Join(",", file.Tags)), $"tags[{index}]");
            }

            index++;
        }

        var response = await _httpClient.PostAsync(
            $"{_configuration["API:BaseUrl"]}/api/galleries/{galleryId}/images", content);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<ImageMetadata>>() 
            ?? new List<ImageMetadata>();
    }

    public async Task<ImageSearchResult> SearchImagesAsync(ImageSearchRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"{_configuration["API:BaseUrl"]}/api/galleries/search", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ImageSearchResult>() 
            ?? new ImageSearchResult();
    }

    public async Task<ImageMetadata?> GetImageMetadataAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<ImageMetadata>(
            $"{_configuration["API:BaseUrl"]}/api/galleries/images/{id}");
    }

    public async Task<bool> UpdateImageMetadataAsync(Guid id, ImageMetadataUpdate update)
    {
        var response = await _httpClient.PatchAsJsonAsync(
            $"{_configuration["API:BaseUrl"]}/api/galleries/images/{id}", update);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteImageAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync(
            $"{_configuration["API:BaseUrl"]}/api/galleries/images/{id}");
        return response.IsSuccessStatusCode;
    }
}

public record FileUploadRequest(Stream Stream, string FileName, string ContentType, string? Description = null, List<string>? Tags = null);

public record ImageSearchRequest
{
    public string? Query { get; init; }
    public List<string>? Tags { get; init; }
    public List<string>? Formats { get; init; }
    public DateTimeOffset? FromDate { get; init; }
    public DateTimeOffset? ToDate { get; init; }
    public int? MinWidth { get; init; }
    public int? MinHeight { get; init; }
    public bool? HasGpsData { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}

public record ImageMetadataUpdate
{
    public string? Description { get; init; }
    public List<string>? Tags { get; init; }
    public Dictionary<string, string>? CustomMetadata { get; init; }
} 