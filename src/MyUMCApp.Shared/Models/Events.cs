namespace MyUMCApp.Shared.Models;

public class ChurchEvent
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string VirtualMeetingUrl { get; set; } = string.Empty;
    public bool IsVirtual { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrencePattern? RecurrencePattern { get; set; }
    public int MaxAttendees { get; set; }
    public decimal? RegistrationFee { get; set; }
    public DateTime? RegistrationDeadline { get; set; }
    public bool RequiresRegistration { get; set; }
    public EventStatus Status { get; set; }
    public List<EventRegistration> Registrations { get; set; } = new();
    public List<EventReminder> Reminders { get; set; } = new();
    public Guid OrganizerId { get; set; }
    public User? Organizer { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RecurrencePattern
{
    public Guid Id { get; set; }
    public RecurrenceType Type { get; set; }
    public int Interval { get; set; }
    public List<DayOfWeek> DaysOfWeek { get; set; } = new();
    public DateTime? EndDate { get; set; }
    public int? Occurrences { get; set; }
}

public class EventRegistration
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public ChurchEvent? Event { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public RegistrationStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }
    public string? Notes { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public string? PaymentReference { get; set; }
}

public class EventReminder
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public ChurchEvent? Event { get; set; }
    public TimeSpan TimeBeforeEvent { get; set; }
    public ReminderType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Sent { get; set; }
    public DateTime? SentAt { get; set; }
}

public enum RecurrenceType
{
    Daily,
    Weekly,
    Monthly,
    Yearly
}

public enum EventStatus
{
    Draft,
    Published,
    Cancelled,
    Completed
}

public enum RegistrationStatus
{
    Pending,
    Confirmed,
    Cancelled,
    WaitListed
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}

public enum ReminderType
{
    Email,
    SMS,
    PushNotification
} 