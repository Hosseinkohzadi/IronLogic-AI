using System.ComponentModel.DataAnnotations;
using IronLogic.Application.Validation;

namespace IronLogic.Application.DTOs;

/// <summary>
///     DTO for logging a daily weight entry. Matches the OpenAPI DailyWeight schema.
/// </summary>
public class DailyWeightRequest
{
    [Required] [NotFutureDate] public DateTime Date { get; set; }

    [Required]
    [Range(40.0, 200.0, ErrorMessage = "Weight must be between 40 and 200 kg.")]
    public float Weight { get; set; }

    [MaxLength(200)] public string? Note { get; set; }
}