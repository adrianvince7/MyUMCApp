using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyUMCApp.Members.API.Services;
using MyUMCApp.Shared.Models.Auth;

namespace MyUMCApp.Members.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileStorageService _storageService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IProfileStorageService storageService,
        ILogger<ProfileController> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    [HttpPost("upload-picture")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Validate file size (5MB max)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest("File size must be less than 5MB.");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Only .jpg, .jpeg, and .png files are allowed.");
            }

            // Generate a unique filename
            var userId = User.FindFirst("sub")?.Value ?? throw new InvalidOperationException("User ID not found in claims");
            var fileName = $"{userId}-{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";

            using var stream = file.OpenReadStream();
            var url = await _storageService.UploadProfilePictureAsync(stream, fileName);

            return Ok(new { url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile picture");
            return StatusCode(500, "An error occurred while uploading the profile picture.");
        }
    }
} 