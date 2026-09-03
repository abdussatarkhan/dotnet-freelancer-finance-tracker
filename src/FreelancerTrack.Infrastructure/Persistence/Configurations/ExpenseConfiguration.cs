using FreelancerTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelancerTrack.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.ProjectId).HasColumnName("project_id");
        builder.Property(e => e.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Vendor).HasColumnName("vendor").HasMaxLength(150);
        builder.Property(e => e.Category).HasColumnName("category").HasConversion<int>().IsRequired();
        builder.Property(e => e.DateUtc).HasColumnName("date_utc").HasColumnType("timestamp with time zone").IsRequired();

        builder.Property(e => e.GrossAmount).HasColumnName("gross_amount").HasPrecision(12, 2).IsRequired();
        builder.Property(e => e.TaxAmount).HasColumnName("tax_amount").HasPrecision(12, 2).IsRequired();
        builder.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(e => e.ExchangeRateToBase).HasColumnName("exchange_rate_to_base").HasPrecision(18, 6).IsRequired();
        builder.Property(e => e.GrossAmountInBaseCurrency).HasColumnName("gross_amount_in_base_currency").HasPrecision(12, 2).IsRequired();

        builder.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasConversion<int>().IsRequired();
        builder.Property(e => e.IsBillable).HasColumnName("is_billable").IsRequired();
        builder.Property(e => e.ReimbursedStatus).HasColumnName("reimbursed_status").HasConversion<int>().IsRequired();

        builder.Property(e => e.InvoiceItemId).HasColumnName("invoice_item_id");
        builder.Property(e => e.InvoicedAtUtc).HasColumnName("invoiced_at_utc").HasColumnType("timestamp with time zone");

        builder.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(e => e.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(e => e.DeletedAtUtc).HasColumnName("deleted_at_utc").HasColumnType("timestamp with time zone");

        // Optimistic concurrency token
        builder.Property(e => e.Version).IsRowVersion();

        // Foreign Key Relationships
        builder.HasOne(e => e.Project)
            .WithMany(p => p.Expenses)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.InvoiceItem)
            .WithOne(ii => ii.Expense)
            .HasForeignKey<Expense>(e => e.InvoiceItemId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for Financial Queries
        builder.HasIndex(e => e.ProjectId).HasDatabaseName("ix_expenses_project_id");
        builder.HasIndex(e => e.DateUtc).HasDatabaseName("ix_expenses_date_utc");
        builder.HasIndex(e => new { e.ProjectId, e.IsBillable, e.ReimbursedStatus })
            .HasDatabaseName("ix_expenses_project_billable_reimbursed");
        builder.HasIndex(e => e.InvoiceItemId).HasDatabaseName("ix_expenses_invoice_item_id");
    }
}
