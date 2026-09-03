using FreelancerTrack.Domain.Common;
using FreelancerTrack.Domain.Enums;

namespace FreelancerTrack.Domain.Entities;

public class RecurringProfile : AuditableEntity
{
    public Guid ClientId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BillingSchedule Schedule { get; set; } = BillingSchedule.Monthly;

    public DateTimeOffset StartDateUtc { get; set; }
    public DateTimeOffset? EndDateUtc { get; set; }
    public DateTimeOffset NextRunDateUtc { get; set; }
    public DateTimeOffset? LastRunDateUtc { get; set; }

    public bool IsActive { get; set; } = true;
    public decimal RetainerAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public int PaymentDueDays { get; set; } = 14;

    // Concurrency Token
    public uint Version { get; set; }

    // Navigation Properties
    public Client Client { get; set; } = null!;
    public Project? Project { get; set; }
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public void AdvanceSchedule()
    {
        LastRunDateUtc = NextRunDateUtc;

        NextRunDateUtc = Schedule switch
        {
            BillingSchedule.Weekly => NextRunDateUtc.AddDays(7),
            BillingSchedule.Monthly => NextRunDateUtc.AddMonths(1),
            BillingSchedule.Quarterly => NextRunDateUtc.AddMonths(3),
            BillingSchedule.Yearly => NextRunDateUtc.AddYears(1),
            _ => throw new ArgumentOutOfRangeException(nameof(Schedule), $"Unsupported schedule {Schedule}")
        };

        if (EndDateUtc.HasValue && NextRunDateUtc > EndDateUtc.Value)
        {
            IsActive = false;
        }
    }
}
