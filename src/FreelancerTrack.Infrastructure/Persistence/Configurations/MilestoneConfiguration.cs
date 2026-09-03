using FreelancerTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelancerTrack.Infrastructure.Persistence.Configurations;

public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    public void Configure(EntityTypeBuilder<Milestone> builder)
    {
        builder.ToTable("milestones");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.ProjectId).HasColumnName("project_id").IsRequired();
        builder.Property(m => m.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(m => m.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(m => m.DeadlineUtc).HasColumnName("deadline_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(m => m.Amount).HasColumnName("amount").HasPrecision(12, 2).IsRequired();
        builder.Property(m => m.Status).HasColumnName("status").HasConversion<int>().IsRequired();

        builder.Property(m => m.InvoiceItemId).HasColumnName("invoice_item_id");
        builder.Property(m => m.InvoicedAtUtc).HasColumnName("invoiced_at_utc").HasColumnType("timestamp with time zone");

        builder.Property(m => m.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(m => m.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(m => m.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(m => m.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        // Optimistic concurrency token
        builder.Property(m => m.Version).IsRowVersion();

        // Foreign Key Relationships
        builder.HasOne(m => m.Project)
            .WithMany(p => p.Milestones)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.InvoiceItem)
            .WithOne(ii => ii.Milestone)
            .HasForeignKey<Milestone>(m => m.InvoiceItemId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(m => m.ProjectId).HasDatabaseName("ix_milestones_project_id");
        builder.HasIndex(m => new { m.ProjectId, m.Status }).HasDatabaseName("ix_milestones_project_id_status");
        builder.HasIndex(m => m.InvoiceItemId).HasDatabaseName("ix_milestones_invoice_item_id");
    }
}
