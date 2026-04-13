using System.Reflection;
using System.Text.RegularExpressions;

using IronLogic.Application.Interfaces;

using Microsoft.Extensions.Logging;

namespace IronLogic.Infrastructure.Services;

/// <summary>
/// Renders local HTML email templates by replacing named placeholders from a model object.
/// </summary>
public partial class EmailTemplateService(ILogger<EmailTemplateService> logger) : IEmailTemplateService
{
    private const string LayoutTemplateName = "Layout";

    /// <inheritdoc />
    public async Task<string> GetRenderedTemplateAsync(
        string templateName,
        object model,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);
        ArgumentNullException.ThrowIfNull(model);

        if (templateName.Contains("..", StringComparison.OrdinalIgnoreCase)
            || templateName.Contains(Path.DirectorySeparatorChar)
            || templateName.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new InvalidOperationException("Invalid template name.");
        }

        var templatesDirectory = Path.Combine(AppContext.BaseDirectory, "Emails", "Templates");
        var templatePath = Path.Combine(templatesDirectory, templateName + ".html");
        if (!File.Exists(templatePath))
        {
            logger.LogError("Email template file not found: {TemplatePath}", templatePath);
            throw new FileNotFoundException("Email template file was not found.", templatePath);
        }

        var bodyTemplate = await File.ReadAllTextAsync(templatePath, cancellationToken);
        var bodyRendered = ReplacePlaceholders(bodyTemplate, model);

        var layoutPath = Path.Combine(templatesDirectory, LayoutTemplateName + ".html");
        if (!File.Exists(layoutPath))
            return bodyRendered;

        var layoutTemplate = await File.ReadAllTextAsync(layoutPath, cancellationToken);
        var layoutModel = new
        {
            Body = bodyRendered,
            Subject = GetPropertyValue(model, "Subject") ?? string.Empty,
            CurrentYear = DateTime.UtcNow.Year.ToString()
        };

        return ReplacePlaceholders(layoutTemplate, layoutModel);
    }

    private static string ReplacePlaceholders(string template, object model)
    {
        return PlaceholderRegex().Replace(template, match =>
        {
            var propertyName = match.Groups[1].Value;
            return GetPropertyValue(model, propertyName) ?? string.Empty;
        });
    }

    private static string? GetPropertyValue(object model, string propertyName)
    {
        var property = model.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

        var value = property?.GetValue(model);
        return value?.ToString();
    }

    [GeneratedRegex(@"\{\{\s*([A-Za-z0-9_]+)\s*\}\}")]
    private static partial Regex PlaceholderRegex();
}
