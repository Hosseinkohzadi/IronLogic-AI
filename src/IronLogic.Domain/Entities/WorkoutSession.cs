namespace IronLogic.Domain.Entities;

/// <summary>
///     Represents a workout session for a user on a specific date, containing one or more exercise sessions.
/// </summary>
public class Session : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    /// <summary>
    ///     Gets or sets the date of the exercise session.
    /// </summary>
    public DateTime Date { get; set; }

    public ICollection<ExerciseSession> ExerciseSessions { get; set; } = new List<ExerciseSession>();

}