using System.Security.Cryptography;
using FreelancerTrack.Application.Common.Interfaces;

namespace FreelancerTrack.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageBasePath;

    public LocalFileStorageService(string? storageBasePath = null)
    {
        _storageBasePath = storageBasePath ?? Path.Combine(AppContext.BaseDirectory, "ReceiptStorage");
        if (!Directory.Exists(_storageBasePath))
        {
            Directory.CreateDirectory(_storageBasePath);
        }
    }

    public async Task<(string StoredFileName, string StoragePath, string Sha256Hash, long FileSizeBytes)> SaveFileAsync(
        Stream stream,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        string extension = Path.GetExtension(originalFileName);
        string storedFileName = $"{Guid.NewGuid():N}{extension}";
        string fullPath = Path.Combine(_storageBasePath, storedFileName);

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        byte[] bytes = memoryStream.ToArray();

        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(bytes);
        string hash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        await File.WriteAllBytesAsync(fullPath, bytes, cancellationToken);

        return (storedFileName, fullPath, hash, bytes.LongLength);
    }

    public Task<(Stream ContentStream, string MimeType, string OriginalFileName)?> GetFileAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(storagePath))
        {
            return Task.FromResult<(Stream ContentStream, string MimeType, string OriginalFileName)?>(null);
        }

        var stream = new FileStream(storagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        string originalFileName = Path.GetFileName(storagePath);
        string mimeType = "application/octet-stream";

        return Task.FromResult<(Stream ContentStream, string MimeType, string OriginalFileName)?>((stream, mimeType, originalFileName));
    }

    public Task<bool> DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(storagePath))
        {
            File.Delete(storagePath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
