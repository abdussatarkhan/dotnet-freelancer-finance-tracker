using FreelancerTrack.Domain.Enums;

namespace FreelancerTrack.Application.Features.Invoices.DTOs;

public record InvoiceItemDto(
    Guid Id,
    Guid InvoiceId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    InvoiceItemType ItemType,
    Guid? MilestoneId,
    Guid? ExpenseId);

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid ClientId,
    string ClientName,
    Guid? ProjectId,
    string? ProjectName,
    Guid? RecurringProfileId,
    DateTimeOffset IssueDateUtc,
    DateTimeOffset DueDateUtc,
    string TransactionCurrency,
    string BaseCurrency,
    decimal ExchangeRateToBase,
    decimal SubTotal,
    decimal TaxRatePercentage,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal TotalAmount,
    decimal TotalAmountInBaseCurrency,
    decimal AmountPaid,
    decimal BalanceDue,
    InvoiceStatus Status,
    string? Notes,
    string? PaymentTerms,
    IReadOnlyList<InvoiceItemDto> Items,
    DateTimeOffset CreatedAtUtc);
