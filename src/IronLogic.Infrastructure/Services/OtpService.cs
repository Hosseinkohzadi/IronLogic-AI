using System.Security.Cryptography;

using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IronLogic.Infrastructure.Services;

/// <summary>
/// Generates and validates one-time passwords for email verification.
/// </summary>
public class OtpService(
    AppDbContext dbContext,
    UserManager<User> userManager,
    ILogger<OtpService> logger) : IOtpService
{
    private const int OtpExpiryMinutes = 10;

    /// <inheritdoc />
    public async Task<(string Code, string Token)> GenerateAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var user = await userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User '{userId}' was not found.");

        // Invalidate any previous unused OTPs for this user
        var existing = await dbContext.UserOtps
            .Where(o => o.UserId == userId && !o.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var old in existing)
            old.IsUsed = true;

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var otp = new UserOtp
        {
            UserId = userId,
            Code = code,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            IsUsed = false
        };

        dbContext.UserOtps.Add(otp);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("OTP generated for user {UserId}. Expires at {ExpiresAt}", userId, otp.ExpiresAt);
        return (code, token);
    }

    /// <inheritdoc />
    public async Task<string?> ValidateAndConsumeAsync(
        string userId,
        string code,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var otp = await dbContext.UserOtps
            .Where(o => o.UserId == userId
                        && o.Code == code
                        && !o.IsUsed
                        && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.DateCreated)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp is null)
        {
            logger.LogWarning("OTP validation failed for user {UserId}. Code invalid or expired.", userId);
            return null;
        }

        otp.IsUsed = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("OTP validated and consumed for user {UserId}", userId);
        return otp.Token;
    }
}
