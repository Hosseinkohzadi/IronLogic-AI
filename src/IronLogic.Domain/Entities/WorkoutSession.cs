namespace IronLogic.Domain.Entities;

public class WorkoutSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime Date { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<WorkoutExercise> Exercises { get; init; } = [];

    public double TotalSessionVolume => Exercises?.Sum(e => e.TotalVolume) ?? 0;
}