using FreelancerTrack.Domain.Common;
using FreelancerTrack.Domain.Enums;

namespace FreelancerTrack.Domain.Entities;

public class Invoice : AuditableEntity, ISoftDeletable
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? RecurringProfileId { get; set; }

    public DateTimeOffset IssueDateUtc { get; set; }
    public DateTimeOffset DueDateUtc { get; set; }

    // Currency snapshotting
    public string TransactionCurrency { get; set; } = "USD";
    public string BaseCurrency { get; set; } = "USD";
    public decimal ExchangeRateToBase { get; set; } = 1.000000m;

    // Financial calculations
    public decimal SubTotal { get; set; }
    public decimal TaxRatePercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalAmountInBaseCurrency { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string? Notes { get; set; }
    public string? PaymentTerms { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }

    // Concurrency Token (PostgreSQL xmin)
    public uint Version { get; set; }

    // Navigation Properties
    public Client Client { get; set; } = null!;
    public Project? Project { get; set; }
    public RecurringProfile? RecurringProfile { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

    public void CalculateTotals()
    {
        SubTotal = Items.Sum(x => x.TotalPrice);
        
        decimal discountedSubtotal = Math.Max(0m, SubTotal - DiscountAmount);
        TaxAmount = Math.Round(discountedSubtotal * (TaxRatePercentage / 100m), 2, MidpointRounding.AwayFromZero);
        TotalAmount = discountedSubtotal + TaxAmount;

        TotalAmountInBaseCurrency = Math.Round(TotalAmount * ExchangeRateToBase, 2, MidpointRounding.AwayFromZero);
        BalanceDue = Math.Max(0m, TotalAmount - AmountPaid);

        SyncPaymentStatus();
    }

    public void RecordPayment(decimal paymentAmount)
    {
        if (paymentAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(paymentAmount), "Payment amount must be greater than zero.");
        }

        if (paymentAmount > BalanceDue)
        {
            throw new InvalidOperationException($"Payment amount ({paymentAmount}) exceeds balance due ({BalanceDue}).");
        }

        AmountPaid += paymentAmount;
        BalanceDue = TotalAmount - AmountPaid;

        SyncPaymentStatus();
    }

    public void SyncPaymentStatus()
    {
        if (Status == InvoiceStatus.Void)
        {
            return;
        }

        if (BalanceDue == 0m && TotalAmount > 0m)
        {
            Status = InvoiceStatus.Paid;
        }
        else if (AmountPaid > 0m && BalanceDue > 0m)
        {
            Status = InvoiceStatus.PartiallyPaid;
        }
        else if (Status != InvoiceStatus.Draft && DateTimeOffset.UtcNow > DueDateUtc && BalanceDue > 0m)
        {
            Status = InvoiceStatus.Overdue;
        }
    }
}
