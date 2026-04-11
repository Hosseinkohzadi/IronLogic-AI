using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Settings;
using Microsoft.Extensions.Options;

namespace IronLogic.Infrastructure.Services.Storage;

/// <summary>
/// Azure Blob Storage implementation for cloud file storage.
/// Handles user-generated exercise images with CDN support for global delivery.
/// </summary>
/// <param name="settings">Azure Storage configuration settings.</param>
public class AzureBlobStorageService(IOptions<AzureStorageSettings> settings) : IFileStorageService
{
    private readonly AzureStorageSettings _settings = settings.Value;

    /// <inheritdoc />
    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType = "image/jpeg")
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (fileStream.Length > _settings.MaxFileSizeBytes)
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {_settings.MaxFileSizeBytes / 1024 / 1024}MB.");

        var blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_settings.ContainerName);

        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(fileName);

        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = contentType,
            CacheControl = "public, max-age=31536000"
        };

        fileStream.Position = 0;

        await blobClient.UploadAsync(fileStream, new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders
        });

        return GetPublicUrl(fileName);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string fileUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileUrl);

        try
        {
            var fileName = ExtractFileNameFromUrl(fileUrl);
            if (string.IsNullOrEmpty(fileName))
                return false;

            var blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            return await blobClient.DeleteIfExistsAsync();
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        return await blobClient.ExistsAsync();
    }

    /// <inheritdoc />
    public string GenerateUniqueFileName(string originalFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalFileName);

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var guid = Guid.NewGuid().ToString("N");
        return $"exercise-{guid}{extension}";
    }

    /// <summary>
    /// Gets the public URL for a blob file, using CDN endpoint if configured.
    /// </summary>
    /// <param name="fileName">The blob file name.</param>
    /// <returns>The public URL (CDN or blob storage URL).</returns>
    private string GetPublicUrl(string fileName)
    {
        if (_settings.UseCdn && !string.IsNullOrWhiteSpace(_settings.CdnEndpoint))
            return $"{_settings.CdnEndpoint.TrimEnd('/')}/{_settings.ContainerName}/{fileName}";

        return $"{_settings.BaseUrl.TrimEnd('/')}/{_settings.ContainerName}/{fileName}";
    }

    /// <summary>
    /// Extracts the file name from a public URL.
    /// </summary>
    /// <param name="fileUrl">The public URL.</param>
    /// <returns>The file name, or null if extraction fails.</returns>
    private string? ExtractFileNameFromUrl(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var segments = uri.Segments;
            return segments.Length > 0 ? segments[^1] : null;
        }
        catch
        {
            return null;
        }
    }
}
