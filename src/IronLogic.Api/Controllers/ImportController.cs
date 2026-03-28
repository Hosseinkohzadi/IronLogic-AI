using IronLogic.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IronLogic.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ImportController(IWorkoutImportService importService) : ControllerBase
{
    [HttpPost("workouts")]
    public async Task<IActionResult> ImportWorkouts(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("لطفاً یک فایل CSV معتبر انتخاب کنید.");

        if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("فقط فایل‌های با پسوند .csv مجاز هستند.");

        await using var stream = file.OpenReadStream();
        await importService.ImportWorkoutsAsync(stream);

        return Ok(new { message = "دیتای تمرینات با موفقیت وارد شد." });
    }
}