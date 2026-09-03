using FreelancerTrack.Domain.Common;
using FreelancerTrack.Domain.Enums;

namespace FreelancerTrack.Domain.Entities;

public class Expense : AuditableEntity, ISoftDeletable
{
    public Guid? ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Vendor { get; set; }
    public ExpenseCategory Category { get; set; } = ExpenseCategory.Software;
    public DateTimeOffset DateUtc { get; set; }

    public decimal GrossAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal ExchangeRateToBase { get; set; } = 1.000000m;
    public decimal GrossAmountInBaseCurrency { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;

    // Billable tracking
    public bool IsBillable { get; set; }
    public ExpenseReimbursedStatus ReimbursedStatus { get; set; } = ExpenseReimbursedStatus.NotBillable;
    public Guid? InvoiceItemId { get; set; }
    public DateTimeOffset? InvoicedAtUtc { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }

    // Concurrency Token
    public uint Version { get; set; }

    // Navigation Properties
    public Project? Project { get; set; }
    public InvoiceItem? InvoiceItem { get; set; }
    public ICollection<ReceiptAttachment> Receipts { get; set; } = new List<ReceiptAttachment>();

    public void CalculateBaseAmount()
    {
        GrossAmountInBaseCurrency = Math.Round(GrossAmount * ExchangeRateToBase, 2, MidpointRounding.AwayFromZero);
    }

    public void LinkToInvoice(Guid invoiceItemId)
    {
        if (!IsBillable)
        {
            throw new InvalidOperationException("Non-billable expenses cannot be linked to invoices.");
        }

        InvoiceItemId = invoiceItemId;
        InvoicedAtUtc = DateTimeOffset.UtcNow;
        ReimbursedStatus = ExpenseReimbursedStatus.Reimbursed;
    }
}
