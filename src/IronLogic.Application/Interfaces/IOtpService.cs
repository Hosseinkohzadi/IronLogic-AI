namespace IronLogic.Application.Interfaces;

/// <summary>
/// Provides operations for generating and consuming one-time passwords used in email verification.
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a six-digit OTP and the associated Identity email-confirmation token for a user.
    /// Any previously unused OTPs for the same user are invalidated.
    /// </summary>
    /// <param name="userId">The target user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple of the six-digit code and the Identity confirmation token.</returns>
    Task<(string Code, string Token)> GenerateAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the provided OTP code for a user and, if valid, marks it as consumed.
    /// </summary>
    /// <param name="userId">The target user identifier.</param>
    /// <param name="code">The six-digit OTP code supplied by the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The Identity confirmation token when the code is valid and unexpired;
    /// <c>null</c> if the code is invalid, expired, or already consumed.
    /// </returns>
    Task<string?> ValidateAndConsumeAsync(string userId, string code, CancellationToken cancellationToken = default);
}
