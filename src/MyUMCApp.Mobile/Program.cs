using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyUMCApp.Mobile.Models;
using MyUMCApp.Mobile.Services;
using System.ComponentModel.DataAnnotations;

namespace MyUMCApp.Mobile;

class Program
{
    private static IAuthService? _authService;
    private static ILogger<Program>? _logger;

    static async Task Main(string[] args)
    {
        // Build configuration
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        
        var configuration = builder.Build();

        // Build host
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddHttpClient<IAuthService, AuthService>();
                services.AddSingleton<IConfiguration>(configuration);
                services.AddLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
            })
            .Build();

        _authService = host.Services.GetRequiredService<IAuthService>();
        _logger = host.Services.GetRequiredService<ILogger<Program>>();

        Console.WriteLine("=== MyUMC Mobile App Demo ===");
        Console.WriteLine("Connecting to API at: " + configuration["ApiSettings:BaseAddress"]);
        Console.WriteLine();

        await RunMobileAppAsync();
    }

    static async Task RunMobileAppAsync()
    {
        while (true)
        {
            Console.WriteLine("\n=== MAIN MENU ===");
            Console.WriteLine("1. Register New User");
            Console.WriteLine("2. Login");
            Console.WriteLine("3. View Profile (requires login)");
            Console.WriteLine("4. Forgot Password");
            Console.WriteLine("5. Reset Password");
            Console.WriteLine("6. Logout");
            Console.WriteLine("7. Check Authentication Status");
            Console.WriteLine("0. Exit");
            Console.Write("\nSelect an option: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await RegisterUserAsync();
                        break;
                    case "2":
                        await LoginUserAsync();
                        break;
                    case "3":
                        await ViewProfileAsync();
                        break;
                    case "4":
                        await ForgotPasswordAsync();
                        break;
                    case "5":
                        await ResetPasswordAsync();
                        break;
                    case "6":
                        await LogoutAsync();
                        break;
                    case "7":
                        await CheckAuthStatusAsync();
                        break;
                    case "0":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                _logger?.LogError(ex, "Unhandled exception in mobile app");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }

    static async Task RegisterUserAsync()
    {
        Console.WriteLine("\n=== USER REGISTRATION ===");
        
        var request = new RegisterRequest();
        
        Console.Write("Email: ");
        request.Email = Console.ReadLine() ?? "";

        Console.Write("Password: ");
        request.Password = ReadPassword();

        Console.Write("Confirm Password: ");
        request.ConfirmPassword = ReadPassword();

        Console.Write("First Name: ");
        request.FirstName = Console.ReadLine() ?? "";

        Console.Write("Last Name: ");
        request.LastName = Console.ReadLine() ?? "";

        Console.Write("Organization (optional): ");
        request.Organization = Console.ReadLine() ?? "";

        Console.Write("Church Role (optional): ");
        request.ChurchRole = Console.ReadLine() ?? "";

        // Validate input
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            Console.WriteLine("\nValidation errors:");
            foreach (var error in validationResults)
            {
                Console.WriteLine($"- {error.ErrorMessage}");
            }
            return;
        }

        var result = await _authService!.RegisterAsync(request);
        
        if (result.Success)
        {
            Console.WriteLine($"\n✓ Registration successful: {result.Message}");
            Console.WriteLine("You can now login with your credentials.");
        }
        else
        {
            Console.WriteLine($"\n✗ Registration failed: {result.Message}");
        }
    }

    static async Task LoginUserAsync()
    {
        Console.WriteLine("\n=== USER LOGIN ===");
        
        var request = new LoginRequest();
        
        Console.Write("Email: ");
        request.Email = Console.ReadLine() ?? "";

        Console.Write("Password: ");
        request.Password = ReadPassword();

        var result = await _authService!.LoginAsync(request);
        
        if (result.Success)
        {
            Console.WriteLine($"\n✓ Login successful: {result.Message}");
            if (result.User != null)
            {
                Console.WriteLine($"Welcome, {result.User.FirstName} {result.User.LastName}!");
                Console.WriteLine($"Organization: {result.User.Organization}");
                Console.WriteLine($"Role: {result.User.ChurchRole}");
            }
        }
        else
        {
            Console.WriteLine($"\n✗ Login failed: {result.Message}");
        }
    }

    static async Task ViewProfileAsync()
    {
        Console.WriteLine("\n=== USER PROFILE ===");
        
        var profile = await _authService!.GetUserProfileAsync();
        
        if (profile != null)
        {
            Console.WriteLine($"ID: {profile.Id}");
            Console.WriteLine($"Email: {profile.Email}");
            Console.WriteLine($"Name: {profile.FirstName} {profile.LastName}");
            Console.WriteLine($"Organization: {profile.Organization}");
            Console.WriteLine($"Church Role: {profile.ChurchRole}");
            Console.WriteLine($"Created: {profile.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Last Login: {profile.LastLogin?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}");
            Console.WriteLine($"Active: {profile.IsActive}");
        }
        else
        {
            Console.WriteLine("✗ Unable to get profile. Please login first.");
        }
    }

    static async Task ForgotPasswordAsync()
    {
        Console.WriteLine("\n=== FORGOT PASSWORD ===");
        
        var request = new ForgotPasswordRequest();
        
        Console.Write("Email: ");
        request.Email = Console.ReadLine() ?? "";

        var result = await _authService!.ForgotPasswordAsync(request);
        
        if (result.Success)
        {
            Console.WriteLine($"\n✓ {result.Message}");
            Console.WriteLine("Note: For this demo, check the API server logs for the reset token.");
        }
        else
        {
            Console.WriteLine($"\n✗ Request failed: {result.Message}");
        }
    }

    static async Task ResetPasswordAsync()
    {
        Console.WriteLine("\n=== RESET PASSWORD ===");
        
        var request = new ResetPasswordRequest();
        
        Console.Write("Email: ");
        request.Email = Console.ReadLine() ?? "";

        Console.Write("Reset Token: ");
        request.Token = Console.ReadLine() ?? "";

        Console.Write("New Password: ");
        request.NewPassword = ReadPassword();

        Console.Write("Confirm New Password: ");
        request.ConfirmPassword = ReadPassword();

        var result = await _authService!.ResetPasswordAsync(request);
        
        if (result.Success)
        {
            Console.WriteLine($"\n✓ Password reset successful: {result.Message}");
        }
        else
        {
            Console.WriteLine($"\n✗ Password reset failed: {result.Message}");
        }
    }

    static async Task LogoutAsync()
    {
        Console.WriteLine("\n=== LOGOUT ===");
        
        var result = await _authService!.LogoutAsync();
        
        if (result.Success)
        {
            Console.WriteLine($"✓ {result.Message}");
        }
        else
        {
            Console.WriteLine($"✗ Logout failed: {result.Message}");
        }
    }

    static async Task CheckAuthStatusAsync()
    {
        Console.WriteLine("\n=== AUTHENTICATION STATUS ===");
        
        var isAuthenticated = await _authService!.IsAuthenticatedAsync();
        var token = _authService.GetStoredToken();
        
        Console.WriteLine($"Authenticated: {(isAuthenticated ? "✓ Yes" : "✗ No")}");
        Console.WriteLine($"Token stored: {(!string.IsNullOrEmpty(token) ? "✓ Yes" : "✗ No")}");
        
        if (!string.IsNullOrEmpty(token))
        {
            Console.WriteLine($"Token preview: {token[..Math.Min(50, token.Length)]}...");
        }
    }

    static string ReadPassword()
    {
        var password = "";
        ConsoleKeyInfo key;
        
        do
        {
            key = Console.ReadKey(true);
            
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[0..^1];
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);
        
        Console.WriteLine();
        return password;
    }
}
