namespace IronLogic.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for user registration
/// </summary>
/// <param name="Email">User's email address (will also be used as username)</param>
/// <param name="Password">User's password (must meet security requirements)</param>
public record RegisterDto(string Email, string Password);
