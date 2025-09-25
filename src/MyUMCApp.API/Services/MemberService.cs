using Microsoft.EntityFrameworkCore;
using MyUMCApp.API.Data;
using MyUMCApp.API.Models;

namespace MyUMCApp.API.Services;

public class MemberService : IMemberService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MemberService> _logger;

    public MemberService(ApplicationDbContext context, ILogger<MemberService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Member>> GetAllMembersAsync()
    {
        return await _context.Members
            .Include(m => m.User)
            .Include(m => m.GivingHistory)
            .Include(m => m.MembershipHistory)
            .OrderBy(m => m.User!.LastName)
            .ThenBy(m => m.User!.FirstName)
            .ToListAsync();
    }

    public async Task<Member?> GetMemberAsync(Guid id)
    {
        return await _context.Members
            .Include(m => m.User)
            .Include(m => m.GivingHistory)
            .Include(m => m.MembershipHistory)
            .Include(m => m.RegisteredEvents)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Member?> GetMemberByUserIdAsync(string userId)
    {
        return await _context.Members
            .Include(m => m.User)
            .Include(m => m.GivingHistory)
            .Include(m => m.MembershipHistory)
            .FirstOrDefaultAsync(m => m.UserId == userId);
    }

    public async Task<Member> CreateMemberAsync(Member member)
    {
        member.Id = Guid.NewGuid();
        member.MemberSince = DateTime.UtcNow;
        member.Status = MembershipStatus.Active;

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new member with ID: {MemberId}", member.Id);
        return member;
    }

    public async Task UpdateMemberAsync(Member member)
    {
        var existingMember = await _context.Members.FindAsync(member.Id);
        if (existingMember == null)
        {
            throw new KeyNotFoundException($"Member with ID {member.Id} not found");
        }

        // Update properties
        existingMember.Organization = member.Organization;
        existingMember.Address = member.Address;
        existingMember.DateOfBirth = member.DateOfBirth;
        existingMember.Status = member.Status;
        existingMember.EmergencyContact = member.EmergencyContact;
        existingMember.EmergencyContactPhone = member.EmergencyContactPhone;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated member with ID: {MemberId}", member.Id);
    }

    public async Task DeleteMemberAsync(Guid id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null)
        {
            throw new KeyNotFoundException($"Member with ID {id} not found");
        }

        _context.Members.Remove(member);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deleted member with ID: {MemberId}", id);
    }

    public async Task AddGivingRecordAsync(Guid memberId, GivingRecord givingRecord)
    {
        var member = await _context.Members.FindAsync(memberId);
        if (member == null)
        {
            throw new KeyNotFoundException($"Member with ID {memberId} not found");
        }

        givingRecord.Id = Guid.NewGuid();
        givingRecord.MemberId = memberId;
        givingRecord.Date = DateTime.UtcNow;

        _context.GivingRecords.Add(givingRecord);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added giving record for member {MemberId}: {Amount}", memberId, givingRecord.Amount);
    }

    public async Task AddMembershipHistoryAsync(Guid memberId, MembershipHistory history)
    {
        var member = await _context.Members.FindAsync(memberId);
        if (member == null)
        {
            throw new KeyNotFoundException($"Member with ID {memberId} not found");
        }

        history.Id = Guid.NewGuid();
        history.MemberId = memberId;

        _context.MembershipHistories.Add(history);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added membership history for member {MemberId}", memberId);
    }

    public async Task RegisterForEventAsync(Guid memberId, Guid eventId)
    {
        var member = await _context.Members.FindAsync(memberId);
        if (member == null)
        {
            throw new KeyNotFoundException($"Member with ID {memberId} not found");
        }

        var churchEvent = await _context.ChurchEvents.FindAsync(eventId);
        if (churchEvent == null)
        {
            throw new KeyNotFoundException($"Event with ID {eventId} not found");
        }

        // Check if already registered
        var existingRegistration = await _context.EventRegistrations
            .FirstOrDefaultAsync(r => r.UserId == member.UserId && r.EventId == eventId);

        if (existingRegistration != null)
        {
            throw new InvalidOperationException("Member is already registered for this event");
        }

        // Check capacity
        if (churchEvent.RequiresRegistration && churchEvent.MaxAttendees > 0)
        {
            var currentRegistrations = await _context.EventRegistrations
                .CountAsync(r => r.EventId == eventId && r.Status == RegistrationStatus.Confirmed);

            if (currentRegistrations >= churchEvent.MaxAttendees)
            {
                throw new InvalidOperationException("Event is at full capacity");
            }
        }

        var registration = new EventRegistration
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = member.UserId,
            Status = RegistrationStatus.Confirmed,
            RegisteredAt = DateTime.UtcNow
        };

        _context.EventRegistrations.Add(registration);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Registered member {MemberId} for event {EventId}", memberId, eventId);
    }
}