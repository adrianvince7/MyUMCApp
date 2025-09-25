using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyUMCApp.Client;
using Blazored.LocalStorage;
using Fluxor;
using MudBlazor.Services;
using MyUMCApp.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add MudBlazor services
builder.Services.AddMudServices();

// Add Fluxor for state management
builder.Services.AddFluxor(options => 
{
    options.ScanAssemblies(typeof(Program).Assembly);
    options.UseReduxDevTools();
});

// Add local storage
builder.Services.AddBlazoredLocalStorage();

// Configure HTTP client for API
var apiBaseAddress = builder.Configuration["ApiSettings:BaseAddress"] ?? "http://localhost:5294/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });

// Add custom authentication service
builder.Services.AddScoped<IApiAuthService, ApiAuthService>();

// Add custom services
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<ITranslationService, TranslationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEventService, EventService>();

await builder.Build().RunAsync();
