using FreelancerTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelancerTrack.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");

        builder.Property(i => i.InvoiceNumber).HasColumnName("invoice_number").HasMaxLength(50).IsRequired();
        builder.Property(i => i.ClientId).HasColumnName("client_id").IsRequired();
        builder.Property(i => i.ProjectId).HasColumnName("project_id");
        builder.Property(i => i.RecurringProfileId).HasColumnName("recurring_profile_id");

        builder.Property(i => i.IssueDateUtc).HasColumnName("issue_date_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(i => i.DueDateUtc).HasColumnName("due_date_utc").HasColumnType("timestamp with time zone").IsRequired();

        // Currencies & Precision
        builder.Property(i => i.TransactionCurrency).HasColumnName("transaction_currency").HasMaxLength(3).IsRequired();
        builder.Property(i => i.BaseCurrency).HasColumnName("base_currency").HasMaxLength(3).IsRequired();
        builder.Property(i => i.ExchangeRateToBase).HasColumnName("exchange_rate_to_base").HasPrecision(18, 6).IsRequired();

        builder.Property(i => i.SubTotal).HasColumnName("sub_total").HasPrecision(12, 2).IsRequired();
        builder.Property(i => i.TaxRatePercentage).HasColumnName("tax_rate_percentage").HasPrecision(5, 2).IsRequired();
        builder.Property(i => i.TaxAmount).HasColumnName("tax_amount").HasPrecision(12, 2).IsRequired();
        builder.Property(i => i.DiscountAmount).HasColumnName("discount_amount").HasPrecision(12, 2).IsRequired();
        builder.Property(i => i.TotalAmount).HasColumnName("total_amount").HasPrecision(12, 2).IsRequired();
        builder.Property(i => i.TotalAmountInBaseCurrency).HasColumnName("total_amount_in_base_currency").HasPrecision(12, 2).IsRequired();
        builder.Property(i => i.AmountPaid).HasColumnName("amount_paid").HasPrecision(12, 2).IsRequired();
        builder.Property(i => i.BalanceDue).HasColumnName("balance_due").HasPrecision(12, 2).IsRequired();

        builder.Property(i => i.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(i => i.Notes).HasColumnName("notes").HasMaxLength(2000);
        builder.Property(i => i.PaymentTerms).HasColumnName("payment_terms").HasMaxLength(500);

        builder.Property(i => i.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(i => i.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(i => i.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(i => i.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        builder.Property(i => i.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(i => i.DeletedAtUtc).HasColumnName("deleted_at_utc").HasColumnType("timestamp with time zone");

        // Optimistic concurrency token
        builder.Property(i => i.Version).IsRowVersion();

        // Foreign Key Relationships
        builder.HasOne(i => i.Client)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Project)
            .WithMany(p => p.Invoices)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.RecurringProfile)
            .WithMany(rp => rp.Invoices)
            .HasForeignKey(i => i.RecurringProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(i => i.InvoiceNumber).HasDatabaseName("ix_invoices_invoice_number").IsUnique();
        builder.HasIndex(i => i.ClientId).HasDatabaseName("ix_invoices_client_id");
        builder.HasIndex(i => i.ProjectId).HasDatabaseName("ix_invoices_project_id");
        builder.HasIndex(i => i.RecurringProfileId).HasDatabaseName("ix_invoices_recurring_profile_id");
        builder.HasIndex(i => new { i.ClientId, i.Status, i.IssueDateUtc }).HasDatabaseName("ix_invoices_client_status_issue_date");
        builder.HasIndex(i => new { i.Status, i.DueDateUtc }).HasDatabaseName("ix_invoices_status_due_date");
    }
}
