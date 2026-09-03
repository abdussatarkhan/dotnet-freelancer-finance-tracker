using FreelancerTrack.Domain.Common;
using FreelancerTrack.Domain.Enums;

namespace FreelancerTrack.Domain.Entities;

public class Milestone : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset DeadlineUtc { get; set; }
    public decimal Amount { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;

    // Linkage to Generated Invoice Line Item
    public Guid? InvoiceItemId { get; set; }
    public DateTimeOffset? InvoicedAtUtc { get; set; }

    // Concurrency Token
    public uint Version { get; set; }

    // Navigation Properties
    public Project Project { get; set; } = null!;
    public InvoiceItem? InvoiceItem { get; set; }

    public void MarkCompleted()
    {
        if (Status == MilestoneStatus.Billed || Status == MilestoneStatus.Paid)
        {
            throw new InvalidOperationException($"Cannot mark milestone as Completed from state {Status}.");
        }
        Status = MilestoneStatus.Completed;
    }

    public void MarkBilled(Guid invoiceItemId)
    {
        if (Status != MilestoneStatus.Completed)
        {
            throw new InvalidOperationException("Only completed milestones can be billed.");
        }
        InvoiceItemId = invoiceItemId;
        InvoicedAtUtc = DateTimeOffset.UtcNow;
        Status = MilestoneStatus.Billed;
    }

    public void MarkPaid()
    {
        if (Status != MilestoneStatus.Billed)
        {
            throw new InvalidOperationException("Milestone must be billed before marking as paid.");
        }
        Status = MilestoneStatus.Paid;
    }
}
