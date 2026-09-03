using FreelancerTrack.Domain.Common;

namespace FreelancerTrack.Domain.Entities;

public class Client : AuditableEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? TaxOrVatNumber { get; set; }
    public string BillingStreet { get; set; } = string.Empty;
    public string BillingCity { get; set; } = string.Empty;
    public string BillingState { get; set; } = string.Empty;
    public string BillingPostalCode { get; set; } = string.Empty;
    public string BillingCountry { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }

    // Concurrency Token (PostgreSQL xmin)
    public uint Version { get; set; }

    // Navigation Properties
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<RecurringProfile> RecurringProfiles { get; set; } = new List<RecurringProfile>();
}
