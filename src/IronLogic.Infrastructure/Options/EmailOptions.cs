namespace IronLogic.Infrastructure.Options;

/// <summary>
/// Provides email provider configuration settings.
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// Gets or sets the SendGrid API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender email address.
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender display name.
    /// </summary>
    public string FromName { get; set; } = "IronLogic AI";
}
