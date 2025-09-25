using MyUMCApp.API.Models;

namespace MyUMCApp.API.Services;

public interface IMemberService
{
    Task<IEnumerable<Member>> GetAllMembersAsync();
    Task<Member?> GetMemberAsync(Guid id);
    Task<Member?> GetMemberByUserIdAsync(string userId);
    Task<Member> CreateMemberAsync(Member member);
    Task UpdateMemberAsync(Member member);
    Task DeleteMemberAsync(Guid id);
    Task AddGivingRecordAsync(Guid memberId, GivingRecord givingRecord);
    Task AddMembershipHistoryAsync(Guid memberId, MembershipHistory history);
    Task RegisterForEventAsync(Guid memberId, Guid eventId);
}