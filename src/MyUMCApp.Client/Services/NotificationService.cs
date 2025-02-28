using Microsoft.JSInterop;
using MudBlazor;

namespace MyUMCApp.Client.Services;

public interface INotificationService
{
    Task InitializeAsync();
    Task SubscribeToNotificationsAsync();
    Task UnsubscribeFromNotificationsAsync();
    Task SendNotificationAsync(string title, string message, NotificationType type);
    Task<bool> RequestPermissionAsync();
    Task<bool> HasPermissionAsync();
}

public enum NotificationType
{
    Event,
    Announcement,
    Sermon,
    Store,
    System
}

public class NotificationService : INotificationService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ISnackbar _snackbar;
    private bool _isInitialized;

    public NotificationService(IJSRuntime jsRuntime, ISnackbar snackbar)
    {
        _jsRuntime = jsRuntime;
        _snackbar = snackbar;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        await _jsRuntime.InvokeVoidAsync("notificationService.initialize");
        _isInitialized = true;
    }

    public async Task SubscribeToNotificationsAsync()
    {
        if (!_isInitialized)
            await InitializeAsync();

        var hasPermission = await HasPermissionAsync();
        if (!hasPermission)
        {
            var granted = await RequestPermissionAsync();
            if (!granted)
            {
                _snackbar.Add("Notifications permission denied", Severity.Warning);
                return;
            }
        }

        await _jsRuntime.InvokeVoidAsync("notificationService.subscribe");
    }

    public async Task UnsubscribeFromNotificationsAsync()
    {
        if (!_isInitialized) return;

        await _jsRuntime.InvokeVoidAsync("notificationService.unsubscribe");
    }

    public async Task SendNotificationAsync(string title, string message, NotificationType type)
    {
        if (!_isInitialized)
            await InitializeAsync();

        var hasPermission = await HasPermissionAsync();
        if (!hasPermission)
        {
            _snackbar.Add(message, MapSeverity(type));
            return;
        }

        var icon = MapTypeToIcon(type);
        await _jsRuntime.InvokeVoidAsync("notificationService.sendNotification", title, message, icon);
    }

    public async Task<bool> RequestPermissionAsync()
    {
        if (!_isInitialized)
            await InitializeAsync();

        return await _jsRuntime.InvokeAsync<bool>("notificationService.requestPermission");
    }

    public async Task<bool> HasPermissionAsync()
    {
        if (!_isInitialized)
            await InitializeAsync();

        return await _jsRuntime.InvokeAsync<bool>("notificationService.hasPermission");
    }

    private static string MapTypeToIcon(NotificationType type)
    {
        return type switch
        {
            NotificationType.Event => "/icons/event.png",
            NotificationType.Announcement => "/icons/announcement.png",
            NotificationType.Sermon => "/icons/sermon.png",
            NotificationType.Store => "/icons/store.png",
            NotificationType.System => "/icons/system.png",
            _ => "/icons/default.png"
        };
    }

    private static Severity MapSeverity(NotificationType type)
    {
        return type switch
        {
            NotificationType.Event => Severity.Info,
            NotificationType.Announcement => Severity.Info,
            NotificationType.Sermon => Severity.Success,
            NotificationType.Store => Severity.Normal,
            NotificationType.System => Severity.Warning,
            _ => Severity.Normal
        };
    }
} 