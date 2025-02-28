using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MyUMCApp.Shared.Models;

namespace MyUMCApp.Client.Services;

public class EventService : IEventService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EventService> _logger;
    private const string BaseUrl = "api/events";

    public EventService(HttpClient httpClient, ILogger<EventService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ChurchEvent>> GetEventsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<ChurchEvent>>(BaseUrl);
            return response ?? new List<ChurchEvent>();
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            return new List<ChurchEvent>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events");
            throw;
        }
    }

    public async Task<ChurchEvent> GetEventAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ChurchEvent>($"{BaseUrl}/{id}");
            if (response == null)
            {
                throw new KeyNotFoundException($"Event with ID {id} not found");
            }
            return response;
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event {EventId}", id);
            throw;
        }
    }

    public async Task<ChurchEvent> CreateEventAsync(ChurchEvent churchEvent)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, churchEvent);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ChurchEvent>() 
                ?? throw new InvalidOperationException("Failed to create event");
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            throw;
        }
    }

    public async Task<ChurchEvent> UpdateEventAsync(ChurchEvent churchEvent)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{churchEvent.Id}", churchEvent);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ChurchEvent>() 
                ?? throw new InvalidOperationException("Failed to update event");
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}", churchEvent.Id);
            throw;
        }
    }

    public async Task DeleteEventAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", id);
            throw;
        }
    }

    public async Task RegisterForEventAsync(Guid eventId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{BaseUrl}/{eventId}/register", null);
            response.EnsureSuccessStatusCode();
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering for event {EventId}", eventId);
            throw;
        }
    }

    public async Task CancelRegistrationAsync(Guid eventId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{eventId}/register");
            response.EnsureSuccessStatusCode();
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling registration for event {EventId}", eventId);
            throw;
        }
    }

    public async Task<bool> IsRegisteredForEventAsync(Guid eventId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<bool>($"{BaseUrl}/{eventId}/is-registered");
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking registration status for event {EventId}", eventId);
            throw;
        }
    }
} 