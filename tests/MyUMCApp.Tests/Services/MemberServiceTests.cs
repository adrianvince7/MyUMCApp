using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyUMCApp.Members.API.Services;
using MyUMCApp.Shared.Data;
using MyUMCApp.Shared.Models;
using Xunit;

namespace MyUMCApp.Tests.Services;

public class MemberServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<MemberService>> _loggerMock;
    private readonly MemberService _service;

    public MemberServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<MemberService>>();
        _service = new MemberService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetMemberAsync_WithValidId_ReturnsMember()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member { Id = memberId, Organization = "Test UMC" };
        var memberRepoMock = new Mock<IRepository<Member>>();
        
        memberRepoMock
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync(member);

        _unitOfWorkMock
            .Setup(x => x.Repository<Member>())
            .Returns(memberRepoMock.Object);

        // Act
        var result = await _service.GetMemberAsync(memberId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(member);
    }

    [Fact]
    public async Task GetMemberAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var memberRepoMock = new Mock<IRepository<Member>>();
        
        memberRepoMock
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync((Member?)null);

        _unitOfWorkMock
            .Setup(x => x.Repository<Member>())
            .Returns(memberRepoMock.Object);

        // Act
        var result = await _service.GetMemberAsync(memberId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateMemberAsync_WithValidData_ReturnsMember()
    {
        // Arrange
        var member = new Member
        {
            UserId = Guid.NewGuid(),
            Organization = "Test UMC",
            Address = "123 Test St",
            DateOfBirth = DateTime.Now.AddYears(-25),
            Status = MembershipStatus.Active
        };

        var memberRepoMock = new Mock<IRepository<Member>>();
        memberRepoMock
            .Setup(x => x.AddAsync(It.IsAny<Member>()))
            .ReturnsAsync(member);

        _unitOfWorkMock
            .Setup(x => x.Repository<Member>())
            .Returns(memberRepoMock.Object);

        // Act
        var result = await _service.CreateMemberAsync(member);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(member);
        memberRepoMock.Verify(x => x.AddAsync(It.IsAny<Member>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMemberAsync_WithValidData_UpdatesSuccessfully()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var existingMember = new Member
        {
            Id = memberId,
            Organization = "Old UMC",
            Status = MembershipStatus.Active
        };

        var updatedMember = new Member
        {
            Id = memberId,
            Organization = "New UMC",
            Status = MembershipStatus.Active
        };

        var memberRepoMock = new Mock<IRepository<Member>>();
        memberRepoMock
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync(existingMember);

        _unitOfWorkMock
            .Setup(x => x.Repository<Member>())
            .Returns(memberRepoMock.Object);

        // Act
        await _service.UpdateMemberAsync(updatedMember);

        // Assert
        memberRepoMock.Verify(x => x.UpdateAsync(It.Is<Member>(m => 
            m.Id == memberId && 
            m.Organization == "New UMC")), Times.Once);
    }

    [Fact]
    public async Task DeleteMemberAsync_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member { Id = memberId };
        var memberRepoMock = new Mock<IRepository<Member>>();
        
        memberRepoMock
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync(member);

        _unitOfWorkMock
            .Setup(x => x.Repository<Member>())
            .Returns(memberRepoMock.Object);

        // Act
        await _service.DeleteMemberAsync(memberId);

        // Assert
        memberRepoMock.Verify(x => x.DeleteAsync(It.Is<Member>(m => m.Id == memberId)), Times.Once);
    }

    [Fact]
    public async Task AddGivingRecordAsync_WithValidData_AddsSuccessfully()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member
        {
            Id = memberId,
            GivingHistory = new List<GivingRecord>()
        };

        var givingRecord = new GivingRecord
        {
            Amount = 100m,
            Purpose = "Tithe",
            PaymentMethod = PaymentMethod.EcoCash,
            Date = DateTime.UtcNow
        };

        var memberRepoMock = new Mock<IRepository<Member>>();
        memberRepoMock
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync(member);

        _unitOfWorkMock
            .Setup(x => x.Repository<Member>())
            .Returns(memberRepoMock.Object);

        // Act
        await _service.AddGivingRecordAsync(memberId, givingRecord);

        // Assert
        memberRepoMock.Verify(x => x.UpdateAsync(It.Is<Member>(m => 
            m.Id == memberId && 
            m.GivingHistory.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task AddMembershipHistoryAsync_WithValidData_AddsSuccessfully()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member
        {
            Id = memberId,
            MembershipHistory = new List<MembershipHistory>()
        };

        var membershipHistory = new MembershipHistory
        {
            PreviousChurch = "Previous UMC",
            StartDate = DateTime.UtcNow.AddYears(-1),
            EndDate = DateTime.UtcNow
        };

        var memberRepoMock = new Mock<IRepository<Member>>();
        memberRepoMock
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync(member);

        _unitOfWorkMock
            .Setup(x => x.Repository<Member>())
            .Returns(memberRepoMock.Object);

        // Act
        await _service.AddMembershipHistoryAsync(memberId, membershipHistory);

        // Assert
        memberRepoMock.Verify(x => x.UpdateAsync(It.Is<Member>(m => 
            m.Id == memberId && 
            m.MembershipHistory.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task RegisterForEventAsync_WithValidData_RegistersSuccessfully()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var member = new Member
        {
            Id = memberId,
            RegisteredEvents = new List<ChurchEvent>()
        };
        var churchEvent = new ChurchEvent
        {
            Id = eventId,
            Title = "Test Event"
        };

        var memberRepoMock = new Mock<IRepository<Member>>();
        memberRepoMock
            .Setup(x => x.GetByIdAsync(memberId))
            .ReturnsAsync(member);

        var eventRepoMock = new Mock<IRepository<ChurchEvent>>();
        eventRepoMock
            .Setup(x => x.GetByIdAsync(eventId))
            .ReturnsAsync(churchEvent);

        _unitOfWorkMock
            .Setup(x => x.Repository<Member>())
            .Returns(memberRepoMock.Object);
        _unitOfWorkMock
            .Setup(x => x.Repository<ChurchEvent>())
            .Returns(eventRepoMock.Object);

        // Act
        await _service.RegisterForEventAsync(memberId, eventId);

        // Assert
        memberRepoMock.Verify(x => x.UpdateAsync(It.Is<Member>(m => 
            m.Id == memberId && 
            m.RegisteredEvents.Count == 1)), Times.Once);
    }
} 