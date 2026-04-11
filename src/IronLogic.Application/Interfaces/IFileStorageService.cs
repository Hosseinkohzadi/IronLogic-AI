namespace IronLogic.Application.Interfaces;

/// <summary>
/// Defines operations for cloud file storage (Azure Blob, AWS S3, etc.).
/// Used for storing user-generated exercise images with CDN support.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file stream to cloud storage and returns the public URL.
    /// </summary>
    /// <param name="fileStream">The file stream to upload.</param>
    /// <param name="fileName">The unique file name (e.g., "exercise-{guid}.jpg").</param>
    /// <param name="contentType">The MIME content type (e.g., "image/jpeg", "image/png").</param>
    /// <returns>The public URL of the uploaded file (CDN URL if enabled).</returns>
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType = "image/jpeg");

    /// <summary>
    /// Deletes a file from cloud storage using its public URL.
    /// </summary>
    /// <param name="fileUrl">The public URL of the file to delete.</param>
    /// <returns>True if deletion was successful; otherwise, false.</returns>
    Task<bool> DeleteAsync(string fileUrl);

    /// <summary>
    /// Checks if a file exists in cloud storage.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns>True if the file exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string fileName);

    /// <summary>
    /// Generates a unique file name for an image upload.
    /// </summary>
    /// <param name="originalFileName">The original file name with extension.</param>
    /// <returns>A unique file name (e.g., "exercise-{guid}.jpg").</returns>
    string GenerateUniqueFileName(string originalFileName);
}
