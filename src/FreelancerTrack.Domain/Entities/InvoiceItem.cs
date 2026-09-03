using FreelancerTrack.Domain.Common;
using FreelancerTrack.Domain.Enums;

namespace FreelancerTrack.Domain.Entities;

public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1m;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public InvoiceItemType ItemType { get; set; } = InvoiceItemType.Custom;

    // Direct linkage to billable source records
    public Guid? MilestoneId { get; set; }
    public Guid? ExpenseId { get; set; }

    // Navigation Properties
    public Invoice Invoice { get; set; } = null!;
    public Milestone? Milestone { get; set; }
    public Expense? Expense { get; set; }

    public void RecalculateTotal()
    {
        TotalPrice = Math.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);
    }
}
