namespace IronLogic.Domain.Settings;

/// <summary>
/// Configuration settings for Azure Blob Storage integration.
/// Used for storing user-generated exercise images globally.
/// </summary>
public class AzureStorageSettings
{
    /// <summary>
    /// Gets or sets the Azure Storage account connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the blob container name for exercise images.
    /// Default: "exercise-images".
    /// </summary>
    public string ContainerName { get; set; } = "exercise-images";

    /// <summary>
    /// Gets or sets the base URL for CDN or blob storage public access.
    /// Example: "https://ironlogic.blob.core.windows.net".
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to enable CDN for global image delivery.
    /// Recommended for international users (Canada, USA, Europe, Australia).
    /// </summary>
    public bool UseCdn { get; set; } = true;

    /// <summary>
    /// Gets or sets the CDN endpoint URL if UseCdn is enabled.
    /// Example: "https://ironlogic.azureedge.net".
    /// </summary>
    public string? CdnEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the maximum file size in bytes for image uploads (default: 5MB).
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
}
