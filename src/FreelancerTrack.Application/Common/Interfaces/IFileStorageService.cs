namespace FreelancerTrack.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<(string StoredFileName, string StoragePath, string Sha256Hash, long FileSizeBytes)> SaveFileAsync(
        Stream stream,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default);

    Task<(Stream ContentStream, string MimeType, string OriginalFileName)?> GetFileAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default);
}
