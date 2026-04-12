namespace IronLogic.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for authentication response containing JWT token and user details
/// </summary>
/// <param name="Token">JWT authentication token</param>
/// <param name="UserId">Unique identifier for the authenticated user</param>
/// <param name="Email">User's email address</param>
/// <param name="UserName">User's username</param>
/// <param name="Role">User's role (e.g., Admin, User)</param>
public record AuthResponseDto(string Token, Guid UserId, string Email, string? UserName, string Role);
