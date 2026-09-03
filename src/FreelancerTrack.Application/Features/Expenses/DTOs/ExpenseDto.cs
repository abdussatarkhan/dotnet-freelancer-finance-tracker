using FreelancerTrack.Domain.Enums;

namespace FreelancerTrack.Application.Features.Expenses.DTOs;

public record ReceiptAttachmentDto(
    Guid Id,
    Guid ExpenseId,
    string OriginalFileName,
    string MimeType,
    long FileSizeBytes,
    string StoragePath,
    string Sha256Hash,
    DateTimeOffset UploadedAtUtc);

public record ExpenseDto(
    Guid Id,
    Guid? ProjectId,
    string? ProjectName,
    string Title,
    string? Vendor,
    ExpenseCategory Category,
    DateTimeOffset DateUtc,
    decimal GrossAmount,
    decimal TaxAmount,
    string Currency,
    decimal ExchangeRateToBase,
    decimal GrossAmountInBaseCurrency,
    PaymentMethod PaymentMethod,
    bool IsBillable,
    ExpenseReimbursedStatus ReimbursedStatus,
    Guid? InvoiceItemId,
    DateTimeOffset? InvoicedAtUtc,
    IReadOnlyList<ReceiptAttachmentDto> Receipts,
    DateTimeOffset CreatedAtUtc);
