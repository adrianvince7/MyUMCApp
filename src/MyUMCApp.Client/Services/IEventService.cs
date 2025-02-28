using MyUMCApp.Shared.Models;

namespace MyUMCApp.Client.Services;

public interface IEventService
{
    Task<List<ChurchEvent>> GetEventsAsync();
    Task<ChurchEvent> GetEventAsync(Guid id);
    Task<ChurchEvent> CreateEventAsync(ChurchEvent churchEvent);
    Task<ChurchEvent> UpdateEventAsync(ChurchEvent churchEvent);
    Task DeleteEventAsync(Guid id);
    Task RegisterForEventAsync(Guid eventId);
    Task CancelRegistrationAsync(Guid eventId);
    Task<bool> IsRegisteredForEventAsync(Guid eventId);
} 