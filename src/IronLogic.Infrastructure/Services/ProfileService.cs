using IronLogic.Application.DTOs.Profile;
using IronLogic.Application.Shared;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace IronLogic.Infrastructure.Services;

/// <summary>
/// Provides profile business operations for retrieving and updating user profile data.
/// </summary>
public class ProfileService(
    AppDbContext dbContext,
    UserManager<User> userManager,
    ILogger<ProfileService> logger) : IProfileService
{
    /// <inheritdoc />
    public async Task<Result<UserProfileResponseDto>> GetProfileAsync(string userId, CancellationToken cancellationToken)
    {ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var user = await userManager.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            logger.LogWarning("Profile retrieval failed because user was not found: {UserId}", userId);
            return Result.Failure<UserProfileResponseDto>("User not found.");
        }

        if (user.Profile != null)
            return Result.Success(MapToDto(user));

        user.Profile = CreateDefaultProfile(user.Id);
        dbContext.UserProfiles.Add(user.Profile);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(user));
    }

    /// <inheritdoc />
    public async Task<Result<UserProfileResponseDto>> UpdateProfileAsync(
        string userId,
        UpdateProfileDto request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(request);

        var user = await userManager.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            logger.LogWarning("Profile update failed because user was not found: {UserId}", userId);
            return Result.Failure<UserProfileResponseDto>("User not found.");
        }

        var profile = user.Profile;
        if (profile == null)
        {
            profile = CreateDefaultProfile(user.Id);

            dbContext.UserProfiles.Add(profile);
            user.Profile = profile;
        }

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            !string.Equals(request.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            user.Email = request.Email;
            user.NormalizedEmail = userManager.NormalizeEmail(request.Email);
        }

        if (!string.IsNullOrWhiteSpace(request.Name) &&
            !string.Equals(request.Name, user.UserName, StringComparison.OrdinalIgnoreCase))
        {
            user.UserName = request.Name;
            user.NormalizedUserName = userManager.NormalizeName(request.Name);
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            user.PhoneNumber = request.PhoneNumber;
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            profile.FirstName = request.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            profile.LastName = request.LastName;
        }

        if (!string.IsNullOrWhiteSpace(request.ProfilePictureUrl))
        {
            profile.ProfilePictureUrl = request.ProfilePictureUrl;
        }

        profile.Gender = request.Gender;
        profile.Bio = request.Bio;
        profile.DateOfBirth = request.DateOfBirth;
        profile.Height = request.Height;
        profile.CurrentWeight = request.CurrentWeight;
        profile.TargetWeight = request.TargetWeight;
        profile.ActivityLevel = request.ActivityLevel;

        var identityUpdateResult = await userManager.UpdateAsync(user);
        if (!identityUpdateResult.Succeeded)
        {
            var errors = string.Join(", ", identityUpdateResult.Errors.Select(e => e.Description));
            logger.LogWarning("Profile update failed for user {UserId}: {Errors}", userId, errors);
            return Result.Failure<UserProfileResponseDto>(errors);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Profile updated successfully for user: {UserId}", userId);
        return Result.Success(MapToDto(user));
    }

    private static UserProfileResponseDto MapToDto(User user)
    {
        return new UserProfileResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            Name = user.UserName,
            FirstName = user.Profile?.FirstName ?? string.Empty,
            LastName = user.Profile?.LastName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            ProfilePictureUrl = user.Profile?.ProfilePictureUrl ?? string.Empty,
            Gender = user.Profile?.Gender ?? Domain.Enums.Gender.Unknown,
            DateOfBirth = user.Profile?.DateOfBirth,
            Height = user.Profile?.Height,
            CurrentWeight = user.Profile?.CurrentWeight,
            TargetWeight = user.Profile?.TargetWeight,
            ActivityLevel = user.Profile?.ActivityLevel ?? Domain.Enums.ActivityLevel.None,
            Bio = user.Profile?.Bio
        };
    }

    private static UserProfile CreateDefaultProfile(string userId)
    {
        return new UserProfile
        {
            UserId = userId,
            Gender = Domain.Enums.Gender.Unknown,
            ActivityLevel = Domain.Enums.ActivityLevel.None,
            Bio = string.Empty
        };
    }
}
