using FreelancerTrack.Domain.Common;

namespace FreelancerTrack.Domain.Entities;

public class ReceiptAttachment : BaseEntity
{
    public Guid ExpenseId { get; set; }
    public string StoredFileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string Sha256Hash { get; set; } = string.Empty;
    public DateTimeOffset UploadedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    // Navigation Properties
    public Expense Expense { get; set; } = null!;
}
