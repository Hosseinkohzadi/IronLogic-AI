namespace IronLogic.Domain.Entities;

public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the date and time when this entity was created.
    /// </summary>
    public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this entity was last modified.
    /// </summary>
    public DateTimeOffset DateModified { get; set; } = DateTimeOffset.UtcNow;
}