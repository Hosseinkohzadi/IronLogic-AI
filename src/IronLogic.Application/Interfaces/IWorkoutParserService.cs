using IronLogic.Application.DTOs.ParsedWorkout;
using IronLogic.Application.Shared;

namespace IronLogic.Application.Interfaces;

/// <summary>
/// Defines the contract for a service that parses raw text into structured workout data.
/// </summary>
public interface IWorkoutParserService
{
    /// <summary>
    /// Parses a raw string containing a workout log into a structured DTO.
    /// </summary>
    /// <param name="rawText">The raw text to parse.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the <see cref="ParsedWorkoutDto"/> if successful,
    /// or an error if parsing fails.
    /// </returns>
    Result<ParsedWorkoutDto> Parse(string rawText);
}