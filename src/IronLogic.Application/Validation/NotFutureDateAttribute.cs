using System.ComponentModel.DataAnnotations;

namespace IronLogic.Application.Validation;

/// <summary>
///     Validates that a DateTime value is not in the future.
///     Used to enforce the business rule that progress entries cannot be logged for future dates.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class NotFutureDateAttribute() : ValidationAttribute("Date cannot be in the future.")
{
    public override bool IsValid(object? value)
    {
        if (value is DateTime date) return date.Date <= DateTime.UtcNow.Date;

        // Let [Required] handle null/missing values
        return true;
    }
}