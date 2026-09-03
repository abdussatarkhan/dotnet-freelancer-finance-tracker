using FreelancerTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelancerTrack.Infrastructure.Persistence.Configurations;

public class RecurringProfileConfiguration : IEntityTypeConfiguration<RecurringProfile>
{
    public void Configure(EntityTypeBuilder<RecurringProfile> builder)
    {
        builder.ToTable("recurring_profiles");

        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id).HasColumnName("id");

        builder.Property(rp => rp.ClientId).HasColumnName("client_id").IsRequired();
        builder.Property(rp => rp.ProjectId).HasColumnName("project_id");
        builder.Property(rp => rp.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(rp => rp.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(rp => rp.Schedule).HasColumnName("schedule").HasConversion<int>().IsRequired();

        builder.Property(rp => rp.StartDateUtc).HasColumnName("start_date_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(rp => rp.EndDateUtc).HasColumnType("timestamp with time zone").HasColumnName("end_date_utc");
        builder.Property(rp => rp.NextRunDateUtc).HasColumnName("next_run_date_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(rp => rp.LastRunDateUtc).HasColumnName("last_run_date_utc").HasColumnType("timestamp with time zone");

        builder.Property(rp => rp.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(rp => rp.RetainerAmount).HasColumnName("retainer_amount").HasPrecision(12, 2).IsRequired();
        builder.Property(rp => rp.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(rp => rp.PaymentDueDays).HasColumnName("payment_due_days").IsRequired();

        builder.Property(rp => rp.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(rp => rp.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(rp => rp.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(rp => rp.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        // Optimistic concurrency token
        builder.Property(rp => rp.Version).IsRowVersion();

        // Foreign Key Relationships
        builder.HasOne(rp => rp.Client)
            .WithMany(c => c.RecurringProfiles)
            .HasForeignKey(rp => rp.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rp => rp.Project)
            .WithMany(p => p.RecurringProfiles)
            .HasForeignKey(rp => rp.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        // Worker Scanning Indexes
        builder.HasIndex(rp => rp.ClientId).HasDatabaseName("ix_recurring_profiles_client_id");
        builder.HasIndex(rp => new { rp.IsActive, rp.NextRunDateUtc }).HasDatabaseName("ix_recurring_profiles_is_active_next_run_date");
    }
}
