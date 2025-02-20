using Microsoft.Extensions.Logging;
using MyUMCApp.Shared.Data;
using MyUMCApp.Shared.Models;

namespace MyUMCApp.Members.API.Services;

public interface IMemberService
{
    Task<Member?> GetMemberAsync(Guid id);
    Task<IEnumerable<Member>> GetAllMembersAsync();
    Task<Member> CreateMemberAsync(Member member);
    Task UpdateMemberAsync(Member member);
    Task DeleteMemberAsync(Guid id);
    Task AddGivingRecordAsync(Guid memberId, GivingRecord givingRecord);
    Task AddMembershipHistoryAsync(Guid memberId, MembershipHistory history);
    Task RegisterForEventAsync(Guid memberId, Guid eventId);
}

public class MemberService : IMemberService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MemberService> _logger;

    public MemberService(IUnitOfWork unitOfWork, ILogger<MemberService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Member?> GetMemberAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.Repository<Member>().GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving member with ID: {MemberId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Member>> GetAllMembersAsync()
    {
        try
        {
            return await _unitOfWork.Repository<Member>().GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all members");
            throw;
        }
    }

    public async Task<Member> CreateMemberAsync(Member member)
    {
        try
        {
            member.MemberSince = DateTime.UtcNow;
            member.Status = MembershipStatus.Active;
            
            var createdMember = await _unitOfWork.Repository<Member>().AddAsync(member);
            _logger.LogInformation("Created new member with ID: {MemberId}", createdMember.Id);
            
            return createdMember;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating member");
            throw;
        }
    }

    public async Task UpdateMemberAsync(Member member)
    {
        try
        {
            var existingMember = await _unitOfWork.Repository<Member>().GetByIdAsync(member.Id);
            if (existingMember == null)
            {
                throw new KeyNotFoundException($"Member with ID {member.Id} not found");
            }

            await _unitOfWork.Repository<Member>().UpdateAsync(member);
            _logger.LogInformation("Updated member with ID: {MemberId}", member.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member with ID: {MemberId}", member.Id);
            throw;
        }
    }

    public async Task DeleteMemberAsync(Guid id)
    {
        try
        {
            var member = await _unitOfWork.Repository<Member>().GetByIdAsync(id);
            if (member == null)
            {
                throw new KeyNotFoundException($"Member with ID {id} not found");
            }

            await _unitOfWork.Repository<Member>().DeleteAsync(member);
            _logger.LogInformation("Deleted member with ID: {MemberId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting member with ID: {MemberId}", id);
            throw;
        }
    }

    public async Task AddGivingRecordAsync(Guid memberId, GivingRecord givingRecord)
    {
        try
        {
            var member = await _unitOfWork.Repository<Member>().GetByIdAsync(memberId);
            if (member == null)
            {
                throw new KeyNotFoundException($"Member with ID {memberId} not found");
            }

            givingRecord.MemberId = memberId;
            givingRecord.Date = DateTime.UtcNow;
            member.GivingHistory.Add(givingRecord);

            await _unitOfWork.Repository<Member>().UpdateAsync(member);
            _logger.LogInformation("Added giving record for member ID: {MemberId}", memberId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding giving record for member ID: {MemberId}", memberId);
            throw;
        }
    }

    public async Task AddMembershipHistoryAsync(Guid memberId, MembershipHistory history)
    {
        try
        {
            var member = await _unitOfWork.Repository<Member>().GetByIdAsync(memberId);
            if (member == null)
            {
                throw new KeyNotFoundException($"Member with ID {memberId} not found");
            }

            history.MemberId = memberId;
            member.MembershipHistory.Add(history);

            await _unitOfWork.Repository<Member>().UpdateAsync(member);
            _logger.LogInformation("Added membership history for member ID: {MemberId}", memberId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding membership history for member ID: {MemberId}", memberId);
            throw;
        }
    }

    public async Task RegisterForEventAsync(Guid memberId, Guid eventId)
    {
        try
        {
            var member = await _unitOfWork.Repository<Member>().GetByIdAsync(memberId);
            if (member == null)
            {
                throw new KeyNotFoundException($"Member with ID {memberId} not found");
            }

            var churchEvent = await _unitOfWork.Repository<ChurchEvent>().GetByIdAsync(eventId);
            if (churchEvent == null)
            {
                throw new KeyNotFoundException($"Event with ID {eventId} not found");
            }

            if (member.RegisteredEvents.Any(e => e.Id == eventId))
            {
                throw new InvalidOperationException($"Member is already registered for event with ID {eventId}");
            }

            member.RegisteredEvents.Add(churchEvent);
            await _unitOfWork.Repository<Member>().UpdateAsync(member);
            _logger.LogInformation("Registered member {MemberId} for event {EventId}", memberId, eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering member {MemberId} for event {EventId}", memberId, eventId);
            throw;
        }
    }
} 