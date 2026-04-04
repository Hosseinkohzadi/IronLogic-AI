using IronLogic.Domain.Interfaces;

namespace IronLogic.Infrastructure.Repositories;

/// <summary>
/// Provides a generic implementation of the repository pattern for entities.
/// </summary>
/// <typeparam name="T">The entity type that inherits from <see cref="BaseEntity"/>.</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    /// <summary>
    /// The database context used for data access operations.
    /// </summary>
    protected readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericRepository{T}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public GenericRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(int id) => 
        await _context.Set<T>().FindAsync(id);

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> ListAllAsync() => 
        await _context.Set<T>().ToListAsync();

    /// <inheritdoc />
    public async Task AddAsync(T entity) => 
        await _context.Set<T>().AddAsync(entity);

    /// <inheritdoc />
    public void Update(T entity)
    {
        _context.Set<T>().Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void Delete(T entity) => 
        _context.Set<T>().Remove(entity);

    /// <inheritdoc />
    public async Task<bool> SaveChangesAsync() => 
        await _context.SaveChangesAsync() > 0;
}