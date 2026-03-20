namespace IronLogic.Domain.Entities;

public class WorkoutExercise
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public List<ExerciseSet> Sets { get; init; } = [];

    public double TotalVolume => Sets?.Sum(s => s.Volume) ?? 0;
}