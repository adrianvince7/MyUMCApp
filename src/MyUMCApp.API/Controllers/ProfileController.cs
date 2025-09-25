using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MyUMCApp.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ILogger<ProfileController> _logger;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(ILogger<ProfileController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new InvalidOperationException("User ID not found in claims");
            var fileName = $"{userId}-{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";

            // Create upload directory if it doesn't exist
            var uploadPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Save the file
            var filePath = Path.Combine(uploadPath, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Return the URL
            var url = $"/uploads/profiles/{fileName}";
            
            _logger.LogInformation("Profile picture uploaded for user {UserId}: {FileName}", 
                SanitizeForLog(userId), SanitizeForLog(fileName));

            return Ok(new { url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile picture");
            return StatusCode(500, "An error occurred while uploading the profile picture.");
        }
    }

    private static string SanitizeForLog(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "[empty]";
        
        // Remove potential log injection characters
        return input.Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ').Trim();
    }
}