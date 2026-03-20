namespace IronLogic.Domain.Entities;

public class ExerciseSet
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int SetOrder { get; set; }

    public double? Weight { get; set; }

    public int? Reps { get; set; }

    public double? RPE { get; set; } // Rate of Perceived Exertion (Optional)

    // Helper property to calculate the total volume for this specific set
    public double Volume => (Weight ?? 0) * (Reps ?? 0);
}