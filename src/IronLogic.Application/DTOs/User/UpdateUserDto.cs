using System.ComponentModel.DataAnnotations;

namespace IronLogic.Application.DTOs.User;

/// <summary>
/// Data transfer object for updating user information
/// </summary>
public record UpdateUserDto
{
    /// <summary>
    /// Gets or sets the user's email address
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string? Email { get; init; }

    /// <summary>
    /// Gets or sets the user's full name
    /// </summary>
    [StringLength(100, ErrorMessage = "Name must be between 1 and 100 characters", MinimumLength = 1)]
    public string? Name { get; init; }

    /// <summary>
    /// Gets or sets the list of roles to assign to the user
    /// </summary>
    public IReadOnlyList<string>? Roles { get; init; }
}
