using System.ComponentModel.DataAnnotations;
using IronLogic.Application.Validation;

namespace IronLogic.Application.DTOs;

/// <summary>
///     DTO for logging muscle measurements. Matches the OpenAPI MuscleMeasurement schema.
/// </summary>
public class MuscleMeasurementRequest
{
    [Required] [NotFutureDate] public DateTime Date { get; set; }

    [Required]
    [Range(20.0, 60.0, ErrorMessage = "Neck must be between 20 and 60 cm.")]
    public float Neck { get; set; }

    [Required]
    [Range(50.0, 180.0, ErrorMessage = "Chest must be between 50 and 180 cm.")]
    public float Chest { get; set; }

    [Required]
    [Range(40.0, 150.0, ErrorMessage = "Waist must be between 40 and 150 cm.")]
    public float Waist { get; set; }

    public float? BicepsLeft { get; set; }

    public float? BicepsRight { get; set; }

    public float? ThighLeft { get; set; }

    public float? ThighRight { get; set; }
}