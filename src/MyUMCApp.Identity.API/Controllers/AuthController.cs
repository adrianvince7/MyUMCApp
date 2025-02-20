using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyUMCApp.Identity.API.Models;
using MyUMCApp.Identity.API.Services;

namespace MyUMCApp.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, token) = await _authService.AuthenticateAsync(request.Email, request.Password);
        
        if (!success)
            return Unauthorized(new { message = token });

        return Ok(new { token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Organization = request.Organization,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var (success, message) = await _authService.RegisterAsync(user, request.Password, request.Role);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class RegisterRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Organization { get; set; }
    public string Role { get; set; }
} 