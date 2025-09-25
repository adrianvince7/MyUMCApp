using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyUMCApp.API.Models;
using MyUMCApp.API.Services;
using System.Security.Claims;

namespace MyUMCApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly ILogger<MembersController> _logger;

    public MembersController(IMemberService memberService, ILogger<MembersController> logger)
    {
        _memberService = memberService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
    {
        try
        {
            var members = await _memberService.GetAllMembersAsync();
            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all members");
            return StatusCode(500, "An error occurred while retrieving members");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Member>> GetMember(Guid id)
    {
        try
        {
            var member = await _memberService.GetMemberAsync(id);
            if (member == null)
            {
                return NotFound($"Member with ID {id} not found");
            }

            return Ok(member);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving member with ID: {MemberId}", id);
            return StatusCode(500, "An error occurred while retrieving the member");
        }
    }

    [HttpGet("by-user/{userId}")]
    public async Task<ActionResult<Member>> GetMemberByUserId(string userId)
    {
        try
        {
            var member = await _memberService.GetMemberByUserIdAsync(userId);
            if (member == null)
            {
                return NotFound($"Member with User ID {userId} not found");
            }

            return Ok(member);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving member with User ID: {UserId}", SanitizeForLog(userId));
            return StatusCode(500, "An error occurred while retrieving the member");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Member>> CreateMember(Member member)
    {
        try
        {
            // Get current user ID from claims
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated");
            }

            member.UserId = currentUserId;
            var createdMember = await _memberService.CreateMemberAsync(member);
            return CreatedAtAction(nameof(GetMember), new { id = createdMember.Id }, createdMember);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating member");
            return StatusCode(500, "An error occurred while creating the member");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMember(Guid id, Member member)
    {
        if (id != member.Id)
        {
            return BadRequest("Member ID mismatch");
        }

        try
        {
            await _memberService.UpdateMemberAsync(member);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member with ID: {MemberId}", id);
            return StatusCode(500, "An error occurred while updating the member");
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator,ChurchLeader")]
    public async Task<IActionResult> DeleteMember(Guid id)
    {
        try
        {
            await _memberService.DeleteMemberAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting member with ID: {MemberId}", id);
            return StatusCode(500, "An error occurred while deleting the member");
        }
    }

    [HttpPost("{id:guid}/giving-records")]
    public async Task<IActionResult> AddGivingRecord(Guid id, GivingRecord givingRecord)
    {
        try
        {
            await _memberService.AddGivingRecordAsync(id, givingRecord);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding giving record for member ID: {MemberId}", id);
            return StatusCode(500, "An error occurred while adding the giving record");
        }
    }

    [HttpPost("{id:guid}/membership-history")]
    public async Task<IActionResult> AddMembershipHistory(Guid id, MembershipHistory history)
    {
        try
        {
            await _memberService.AddMembershipHistoryAsync(id, history);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding membership history for member ID: {MemberId}", id);
            return StatusCode(500, "An error occurred while adding the membership history");
        }
    }

    [HttpPost("{id:guid}/register-event/{eventId:guid}")]
    public async Task<IActionResult> RegisterForEvent(Guid id, Guid eventId)
    {
        try
        {
            await _memberService.RegisterForEventAsync(id, eventId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering member {MemberId} for event {EventId}", id, eventId);
            return StatusCode(500, "An error occurred while registering for the event");
        }
    }

    private static string SanitizeForLog(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "[empty]";
        
        // Remove potential log injection characters
        return input.Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ').Trim();
    }
}