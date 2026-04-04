using IronLogic.Domain.Entities;

namespace IronLogic.Domain.Interfaces;

/// <summary>
/// Defines a generic repository interface for basic CRUD operations on entities.
/// </summary>
/// <typeparam name="T">The entity type that inherits from <see cref="BaseEntity"/>.</typeparam>
public interface IGenericRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <returns>A read-only list of all entities.</returns>
    Task<IReadOnlyList<T>> ListAllAsync();

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    Task AddAsync(T entity);

    /// <summary>
    /// Marks an existing entity as modified.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(T entity);

    /// <summary>
    /// Marks an entity for deletion.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(T entity);

    /// <summary>
    /// Persists all pending changes to the data store.
    /// </summary>
    /// <returns>True if changes were saved successfully; otherwise, false.</returns>
    Task<bool> SaveChangesAsync();
}