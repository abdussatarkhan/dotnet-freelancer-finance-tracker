using FreelancerTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelancerTrack.Infrastructure.Persistence.Configurations;

public class ReceiptAttachmentConfiguration : IEntityTypeConfiguration<ReceiptAttachment>
{
    public void Configure(EntityTypeBuilder<ReceiptAttachment> builder)
    {
        builder.ToTable("receipt_attachments");

        builder.HasKey(ra => ra.Id);
        builder.Property(ra => ra.Id).HasColumnName("id");

        builder.Property(ra => ra.ExpenseId).HasColumnName("expense_id").IsRequired();
        builder.Property(ra => ra.StoredFileName).HasColumnName("stored_file_name").HasMaxLength(255).IsRequired();
        builder.Property(ra => ra.OriginalFileName).HasColumnName("original_file_name").HasMaxLength(255).IsRequired();
        builder.Property(ra => ra.MimeType).HasColumnName("mime_type").HasMaxLength(100).IsRequired();
        builder.Property(ra => ra.FileSizeBytes).HasColumnName("file_size_bytes").IsRequired();
        builder.Property(ra => ra.StoragePath).HasColumnName("storage_path").HasMaxLength(1000).IsRequired();
        builder.Property(ra => ra.Sha256Hash).HasColumnName("sha256_hash").HasMaxLength(64).IsRequired();
        builder.Property(ra => ra.UploadedAtUtc).HasColumnName("uploaded_at_utc").HasColumnType("timestamp with time zone").IsRequired();

        // Cascade delete attachments when expense is deleted
        builder.HasOne(ra => ra.Expense)
            .WithMany(e => e.Receipts)
            .HasForeignKey(ra => ra.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ra => ra.ExpenseId).HasDatabaseName("ix_receipt_attachments_expense_id");
        builder.HasIndex(ra => ra.Sha256Hash).HasDatabaseName("ix_receipt_attachments_sha256_hash");
    }
}
