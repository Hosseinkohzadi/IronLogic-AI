using IronLogic.Domain.Entities;
using IronLogic.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers.Admin;

/// <summary>
///     Administrative controller for managing exercises.
/// </summary>
[ApiController]
[Route("api/v1/admin/exercises")] // Added v1 for consistency with frontend
[Produces("application/json")]
public class AdminExerciseController(IGenericRepository<Exercise> repository) : ControllerBase
{
    /// <summary>
    ///     Retrieves all exercises from the database with pagination support.
    /// </summary>
    /// <param name="pageNumber">The page number (default: 1).</param>
    /// <param name="pageSize">The number of items per page (default: 20).</param>
    /// <returns>A paginated list of exercises with total count.</returns>
    /// <response code="200">Returns the list of exercises with pagination metadata.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetExercises(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var allExercises = await repository.ListAllAsync();

        // 1. Calculate total count for dashboard display
        var totalCount = allExercises.Count();

        // 2. Separate data for the current page (Paging logic)
        var items = allExercises
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // 3. Return both together
        return Ok(new { TotalCount = totalCount, Items = items });
    }

    /// <summary>
    ///     Retrieves a specific exercise by its unique identifier.
    /// </summary>
    /// <param name="id">The exercise ID.</param>
    /// <returns>The requested exercise.</returns>
    /// <response code="200">Returns the exercise.</response>
    /// <response code="404">Exercise not found.</response>
    [HttpGet("{id:guid}")] // Use guid constraint for identifiers
    [ProducesResponseType<Exercise>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Exercise>> GetExercise(Guid id)
    {
        var exercise = await repository.GetByIdAsync(id);
        return exercise == null ? NotFound() : Ok(exercise);
    }

    /// <summary>
    ///     Creates a new exercise in the database.
    /// </summary>
    /// <param name="exercise">The exercise entity to create.</param>
    /// <returns>The created exercise with its generated ID.</returns>
    /// <response code="201">Exercise created successfully.</response>
    /// <response code="400">Invalid exercise data.</response>
    [HttpPost]
    [ProducesResponseType<Exercise>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Exercise>> CreateExercise([FromBody] Exercise exercise)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await repository.AddAsync(exercise);
        await repository.SaveChangesAsync();
        return CreatedAtAction(nameof(GetExercise), new { id = exercise.Id }, exercise);
    }

    /// <summary>
    ///     Deletes an exercise from the database.
    /// </summary>
    /// <param name="id">The exercise ID.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Exercise deleted successfully.</response>
    /// <response code="404">Exercise not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExercise(Guid id)
    {
        var exercise = await repository.GetByIdAsync(id);
        if (exercise == null) return NotFound();
        repository.Delete(exercise);
        await repository.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    ///     Bulk imports exercises from a JSON payload.
    /// </summary>
    /// <param name="exercises">List of exercises to import.</param>
    /// <returns>Number of exercises imported.</returns>
    /// <response code="200">Exercises imported successfully.</response>
    /// <response code="400">Invalid exercise data.</response>
    [HttpPost("bulk-import")]
    [ProducesResponseType<object>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> BulkImport([FromBody] List<Exercise> exercises)
    {
        if (exercises == null || !exercises.Any()) return BadRequest("No exercises provided");
        foreach (var exercise in exercises) await repository.AddAsync(exercise);
        await repository.SaveChangesAsync();
        return Ok(new { ImportedCount = exercises.Count, Message = "Exercises imported successfully" });
    }
}