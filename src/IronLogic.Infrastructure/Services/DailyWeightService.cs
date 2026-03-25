namespace IronLogic.Infrastructure.Services;

/// <summary>
///     Infrastructure service responsible for persisting daily bodyweight entries.
/// </summary>
/// <remarks>
///     This implementation uses <see cref="AppDbContext" /> to add and save <see cref="DailyWeight" />
///     entities. Validation of the incoming DTO is the responsibility of higher layers (controller / model
///     validation), but this service defends against a <c>null</c> request parameter.
/// </remarks>
public class DailyWeightService(AppDbContext dbContext) : IDailyWeightService
{
    /// <summary>
    ///     Logs a daily weight entry to the application's database.
    /// </summary>
    /// <param name="request">
    ///     The <see cref="DailyWeightRequest" /> containing the date, weight (kg) and an optional note.
    ///     The DTO is expected to conform to the OpenAPI constraints (date not in the future, weight between 40 and 200).
    /// </param>
    /// <returns>
    ///     The persisted <see cref="DailyWeight" /> entity including generated Id.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request" /> is <c>null</c>.</exception>
    /// <remarks>
    ///     The method performs a simple mapping from the request DTO to the domain entity, adds it to the
    ///     <see cref="AppDbContext" />, and calls <c>SaveChangesAsync</c>. Any exceptions raised by EF Core
    ///     (for example, database connectivity errors) will propagate to the caller.
    /// </remarks>
    public async Task<DailyWeight> LogWeightAsync(DailyWeightRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var entry = new DailyWeight
        {
            Date = request.Date,
            Weight = request.Weight,
            Note = request.Note
        };

        dbContext.DailyWeights.Add(entry);
        await dbContext.SaveChangesAsync();

        return entry;
    }
}