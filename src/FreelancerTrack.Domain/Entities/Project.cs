using FreelancerTrack.Domain.Common;
using FreelancerTrack.Domain.Enums;

namespace FreelancerTrack.Domain.Entities;

public class Project : AuditableEntity, ISoftDeletable
{
    public Guid ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal TotalBudget { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    public DateTimeOffset StartDateUtc { get; set; }
    public DateTimeOffset? EndDateUtc { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }

    // Concurrency Token
    public uint Version { get; set; }

    // Navigation Properties
    public Client Client { get; set; } = null!;
    public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<RecurringProfile> RecurringProfiles { get; set; } = new List<RecurringProfile>();
}
