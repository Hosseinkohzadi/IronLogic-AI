namespace IronLogic.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for user login
/// </summary>
/// <param name="Email">User's email address</param>
/// <param name="Password">User's password</param>
public record LoginDto(string Email, string Password);
