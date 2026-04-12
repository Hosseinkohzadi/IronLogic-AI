using IronLogic.Application.DTOs.Profile;
using IronLogic.Application.Shared;

namespace IronLogic.Application.Interfaces;

/// <summary>
/// Defines business operations for retrieving and updating user profiles.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Retrieves the identity and profile details for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a <see cref="Result{T}"/> with <see cref="UserProfileResponseDto"/> on success, or an error on failure.
    /// </returns>
    Task<Result<UserProfileResponseDto>> GetProfileAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates profile details for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="request">The profile update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a <see cref="Result{T}"/> with the updated <see cref="UserProfileResponseDto"/> on success, or an error on failure.
    /// </returns>
    Task<Result<UserProfileResponseDto>> UpdateProfileAsync(
        string userId,
        UpdateProfileDto request,
        CancellationToken cancellationToken);
}
