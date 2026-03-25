namespace IronLogic.Application.DTOs;

/// <summary>
/// DTO containing coaching advice returned by the Coach endpoints.
/// </summary>
public sealed class CoachAdviceResponse
{
    public string Advice { get; set; } = string.Empty;
}