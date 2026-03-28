namespace IronLogic.Application.Interfaces;

public interface IWorkoutImportService
{
    Task ImportWorkoutsAsync(Stream fileStream);
}