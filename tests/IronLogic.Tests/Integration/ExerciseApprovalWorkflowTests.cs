using IronLogic.Application.Services;
using IronLogic.Domain.Entities;
using IronLogic.Domain.Enums;
using IronLogic.Domain.Interfaces;
using IronLogic.Infrastructure.Data;
using IronLogic.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace IronLogic.Tests.Integration;

/// <summary>
/// Integration tests for Exercise Approval workflow.
/// </summary>
public class ExerciseApprovalWorkflowTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IExerciseRepository _exerciseRepository;
    private readonly AdminService _adminService;
    private readonly string _testUserId = "test-user-123";

    public ExerciseApprovalWorkflowTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _exerciseRepository = new ExerciseRepository(_context);
        
        var mockUserMetricsRepository = new Mock<IUserMetricsRepository>();
        _adminService = new AdminService(_exerciseRepository, mockUserMetricsRepository.Object);
    }

    [Fact]
    public async Task ApproveExercise_ShouldSetStatusToApprovedAndIsGlobalTrue()
    {
        // Arrange
        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            Name = "Test Exercise",
            CreatorUserId = _testUserId,
            Status = ExerciseStatus.PendingApproval,
            IsGlobal = false,
            PrimaryMuscleId = Guid.NewGuid(),
            EquipmentId = Guid.NewGuid(),
            Type = ExerciseType.WeightAndReps,
            Url = "https://test.com",
            Mechanics = "Compound",
            Instructions = "Test instructions"
        };

        await _exerciseRepository.AddAsync(exercise);
        await _exerciseRepository.SaveChangesAsync();

        // Act
        var result = await _adminService.ApproveExerciseAsync(exercise.Id);

        // Assert
        Assert.True(result);
        var approvedExercise = await _exerciseRepository.GetByIdAsync(exercise.Id);
        Assert.NotNull(approvedExercise);
        Assert.Equal(ExerciseStatus.Approved, approvedExercise.Status);
        Assert.True(approvedExercise.IsGlobal);
    }

    [Fact]
    public async Task RejectExercise_ShouldSetStatusToRejectedAndIsGlobalFalse()
    {
        // Arrange
        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            Name = "Test Exercise",
            CreatorUserId = _testUserId,
            Status = ExerciseStatus.PendingApproval,
            IsGlobal = false,
            PrimaryMuscleId = Guid.NewGuid(),
            EquipmentId = Guid.NewGuid(),
            Type = ExerciseType.WeightAndReps,
            Url = "https://test.com",
            Mechanics = "Compound",
            Instructions = "Test instructions"
        };

        await _exerciseRepository.AddAsync(exercise);
        await _exerciseRepository.SaveChangesAsync();

        // Act
        var result = await _adminService.RejectExerciseAsync(exercise.Id, "Quality standards not met");

        // Assert
        Assert.True(result);
        var rejectedExercise = await _exerciseRepository.GetByIdAsync(exercise.Id);
        Assert.NotNull(rejectedExercise);
        Assert.Equal(ExerciseStatus.Rejected, rejectedExercise.Status);
        Assert.False(rejectedExercise.IsGlobal);
    }

    [Fact]
    public async Task GetAvailableExercises_ShouldReturnApprovedAndUserPrivateExercises()
    {
        // Arrange
        var approvedExercise = CreateTestExercise("Approved Exercise", "other-user", ExerciseStatus.Approved, true);
        var userPrivateExercise = CreateTestExercise("User Private", _testUserId, ExerciseStatus.Private, false);
        var otherUserPrivateExercise = CreateTestExercise("Other User Private", "other-user", ExerciseStatus.Private, false);
        var pendingExercise = CreateTestExercise("Pending", _testUserId, ExerciseStatus.PendingApproval, false);

        await _exerciseRepository.AddAsync(approvedExercise);
        await _exerciseRepository.AddAsync(userPrivateExercise);
        await _exerciseRepository.AddAsync(otherUserPrivateExercise);
        await _exerciseRepository.AddAsync(pendingExercise);
        await _exerciseRepository.SaveChangesAsync();

        // Act
        var availableExercises = await _exerciseRepository.GetAvailableExercisesAsync(_testUserId);

        // Assert
        Assert.Equal(3, availableExercises.Count); // Approved + User's Private + User's Pending
        Assert.Contains(availableExercises, e => e.Id == approvedExercise.Id);
        Assert.Contains(availableExercises, e => e.Id == userPrivateExercise.Id);
        Assert.Contains(availableExercises, e => e.Id == pendingExercise.Id);
        Assert.DoesNotContain(availableExercises, e => e.Id == otherUserPrivateExercise.Id);
    }

    [Fact]
    public async Task GetPendingApprovals_ShouldReturnOnlyPendingExercises()
    {
        // Arrange
        var pendingExercise1 = CreateTestExercise("Pending 1", _testUserId, ExerciseStatus.PendingApproval, false);
        var pendingExercise2 = CreateTestExercise("Pending 2", "other-user", ExerciseStatus.PendingApproval, false);
        var approvedExercise = CreateTestExercise("Approved", _testUserId, ExerciseStatus.Approved, true);
        var privateExercise = CreateTestExercise("Private", _testUserId, ExerciseStatus.Private, false);

        await _exerciseRepository.AddAsync(pendingExercise1);
        await _exerciseRepository.AddAsync(pendingExercise2);
        await _exerciseRepository.AddAsync(approvedExercise);
        await _exerciseRepository.AddAsync(privateExercise);
        await _exerciseRepository.SaveChangesAsync();

        // Act
        var pendingExercises = await _exerciseRepository.GetPendingApprovalsAsync();

        // Assert
        Assert.Equal(2, pendingExercises.Count);
        Assert.All(pendingExercises, e => Assert.Equal(ExerciseStatus.PendingApproval, e.Status));
    }

    [Fact]
    public async Task GetExercisesByCreator_ShouldReturnOnlyCreatorExercises()
    {
        // Arrange
        var userExercise1 = CreateTestExercise("User Exercise 1", _testUserId, ExerciseStatus.Private, false);
        var userExercise2 = CreateTestExercise("User Exercise 2", _testUserId, ExerciseStatus.Approved, true);
        var otherUserExercise = CreateTestExercise("Other User Exercise", "other-user", ExerciseStatus.Private, false);

        await _exerciseRepository.AddAsync(userExercise1);
        await _exerciseRepository.AddAsync(userExercise2);
        await _exerciseRepository.AddAsync(otherUserExercise);
        await _exerciseRepository.SaveChangesAsync();

        // Act
        var userExercises = await _exerciseRepository.GetExercisesByCreatorAsync(_testUserId);

        // Assert
        Assert.Equal(2, userExercises.Count);
        Assert.All(userExercises, e => Assert.Equal(_testUserId, e.CreatorUserId));
    }

    [Fact]
    public async Task ApproveExercise_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _adminService.ApproveExerciseAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExerciseApprovalWorkflow_EndToEnd()
    {
        // Step 1: User creates private exercise
        var exercise = CreateTestExercise("E2E Test Exercise", _testUserId, ExerciseStatus.Private, false);
        await _exerciseRepository.AddAsync(exercise);
        await _exerciseRepository.SaveChangesAsync();

        // Step 2: Verify only user can see it
        var userExercises = await _exerciseRepository.GetAvailableExercisesAsync(_testUserId);
        Assert.Contains(userExercises, e => e.Id == exercise.Id);

        var otherUserExercises = await _exerciseRepository.GetAvailableExercisesAsync("other-user");
        Assert.DoesNotContain(otherUserExercises, e => e.Id == exercise.Id);

        // Step 3: User submits for approval
        exercise.Status = ExerciseStatus.PendingApproval;
        _exerciseRepository.Update(exercise);
        await _exerciseRepository.SaveChangesAsync();

        // Step 4: Admin sees it in pending approvals
        var pendingApprovals = await _exerciseRepository.GetPendingApprovalsAsync();
        Assert.Contains(pendingApprovals, e => e.Id == exercise.Id);

        // Step 5: Admin approves
        var approvalResult = await _adminService.ApproveExerciseAsync(exercise.Id);
        Assert.True(approvalResult);

        // Step 6: Verify it's now global
        var approvedExercise = await _exerciseRepository.GetByIdAsync(exercise.Id);
        Assert.Equal(ExerciseStatus.Approved, approvedExercise!.Status);
        Assert.True(approvedExercise.IsGlobal);

        // Step 7: Verify all users can now see it
        var allUsersExercises = await _exerciseRepository.GetAvailableExercisesAsync("any-user");
        Assert.Contains(allUsersExercises, e => e.Id == exercise.Id);
    }

    private Exercise CreateTestExercise(string name, string creatorUserId, ExerciseStatus status, bool isGlobal)
    {
        return new Exercise
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatorUserId = creatorUserId,
            Status = status,
            IsGlobal = isGlobal,
            PrimaryMuscleId = Guid.NewGuid(),
            EquipmentId = Guid.NewGuid(),
            Type = ExerciseType.WeightAndReps,
            Url = "https://test.com",
            Mechanics = "Compound",
            Instructions = "Test instructions"
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
