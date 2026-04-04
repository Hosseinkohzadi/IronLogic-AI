using IronLogic.Domain.Entities;
using IronLogic.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/exercises")] // اضافه شدن v1 برای هماهنگی با فرانت‌اِند
[Produces("application/json")]
public class AdminExerciseController(IGenericRepository<Exercise> repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetExercises(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var allExercises = await repository.ListAllAsync();

        // ۱. محاسبه تعداد کل برای نمایش در داشبورد
        var totalCount = allExercises.Count();

        // ۲. جدا کردن دیتای مربوط به صفحه فعلی (Paging logic)
        var items = allExercises
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // ۳. بازگشت هر دو مورد به صورت یکجا
        return Ok(new { TotalCount = totalCount, Items = items });
    }

    [HttpGet("{id:guid}")] // استفاده از محدودیت guid برای شناسه‌ها
    public async Task<ActionResult<Exercise>> GetExercise(Guid id)
    {
        var exercise = await repository.GetByIdAsync(id);
        return exercise == null ? NotFound() : Ok(exercise);
    }

    [HttpPost]
    public async Task<ActionResult<Exercise>> CreateExercise([FromBody] Exercise exercise)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await repository.AddAsync(exercise);
        await repository.SaveChangesAsync();
        return CreatedAtAction(nameof(GetExercise), new { id = exercise.Id }, exercise);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteExercise(Guid id)
    {
        var exercise = await repository.GetByIdAsync(id);
        if (exercise == null) return NotFound();
        repository.Delete(exercise);
        await repository.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("bulk-import")]
    public async Task<ActionResult> BulkImport([FromBody] List<Exercise> exercises)
    {
        if (exercises == null || !exercises.Any()) return BadRequest("No exercises provided");
        foreach (var exercise in exercises) await repository.AddAsync(exercise);
        await repository.SaveChangesAsync();
        return Ok(new { ImportedCount = exercises.Count, Message = "Exercises imported successfully" });
    }
}