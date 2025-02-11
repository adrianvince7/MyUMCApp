namespace MyUMCApp.Shared.Models;

public class Member
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Organization { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime MemberSince { get; set; }
    public MembershipStatus Status { get; set; }
    public string EmergencyContact { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public List<GivingRecord> GivingHistory { get; set; } = new();
    public List<ChurchEvent> RegisteredEvents { get; set; } = new();
    public List<MembershipHistory> MembershipHistory { get; set; } = new();
}

public enum MembershipStatus
{
    Active,
    Inactive,
    Suspended,
    Transferred
}

public class GivingRecord
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public decimal Amount { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public enum PaymentMethod
{
    EcoCash,
    OneMoney,
    Paynow,
    BankTransfer,
    Cash
}

public class MembershipHistory
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string PreviousChurch { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
} 