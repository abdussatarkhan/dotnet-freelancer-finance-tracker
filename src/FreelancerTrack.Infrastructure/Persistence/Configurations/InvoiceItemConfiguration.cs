using FreelancerTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelancerTrack.Infrastructure.Persistence.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items");

        builder.HasKey(ii => ii.Id);
        builder.Property(ii => ii.Id).HasColumnName("id");

        builder.Property(ii => ii.InvoiceId).HasColumnName("invoice_id").IsRequired();
        builder.Property(ii => ii.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        builder.Property(ii => ii.Quantity).HasColumnName("quantity").HasPrecision(12, 2).IsRequired();
        builder.Property(ii => ii.UnitPrice).HasColumnName("unit_price").HasPrecision(12, 2).IsRequired();
        builder.Property(ii => ii.TotalPrice).HasColumnName("total_price").HasPrecision(12, 2).IsRequired();
        builder.Property(ii => ii.ItemType).HasColumnName("item_type").HasConversion<int>().IsRequired();

        builder.Property(ii => ii.MilestoneId).HasColumnName("milestone_id");
        builder.Property(ii => ii.ExpenseId).HasColumnName("expense_id");

        // Relationships
        builder.HasOne(ii => ii.Invoice)
            .WithMany(i => i.Items)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ii => ii.InvoiceId).HasDatabaseName("ix_invoice_items_invoice_id");
        builder.HasIndex(ii => ii.MilestoneId).HasDatabaseName("ix_invoice_items_milestone_id");
        builder.HasIndex(ii => ii.ExpenseId).HasDatabaseName("ix_invoice_items_expense_id");
    }
}
