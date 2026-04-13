namespace IronLogic.Application.Interfaces;

/// <summary>
/// Provides HTML email template rendering operations.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Renders a template file by replacing placeholders with values from the provided model.
    /// </summary>
    /// <param name="templateName">The template file name without extension.</param>
    /// <param name="model">Template model object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Rendered HTML content.</returns>
    Task<string> GetRenderedTemplateAsync(string templateName, object model, CancellationToken cancellationToken = default);
}
