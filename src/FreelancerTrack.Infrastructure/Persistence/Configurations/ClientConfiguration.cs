using FreelancerTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelancerTrack.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(c => c.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(c => c.TaxOrVatNumber).HasColumnName("tax_or_vat_number").HasMaxLength(50);
        builder.Property(c => c.BillingStreet).HasColumnName("billing_street").HasMaxLength(200).IsRequired();
        builder.Property(c => c.BillingCity).HasColumnName("billing_city").HasMaxLength(100).IsRequired();
        builder.Property(c => c.BillingState).HasColumnName("billing_state").HasMaxLength(100).IsRequired();
        builder.Property(c => c.BillingPostalCode).HasColumnName("billing_postal_code").HasMaxLength(20).IsRequired();
        builder.Property(c => c.BillingCountry).HasColumnName("billing_country").HasMaxLength(100).IsRequired();
        builder.Property(c => c.DefaultCurrency).HasColumnName("default_currency").HasMaxLength(3).IsRequired();
        builder.Property(c => c.IsActive).HasColumnName("is_active").IsRequired();

        builder.Property(c => c.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(c => c.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        builder.Property(c => c.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(c => c.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);

        builder.Property(c => c.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(c => c.DeletedAtUtc).HasColumnName("deleted_at_utc").HasColumnType("timestamp with time zone");

        // Optimistic concurrency token
        builder.Property(c => c.Version).IsRowVersion();

        // Indexes
        builder.HasIndex(c => c.Email).HasDatabaseName("ix_clients_email").IsUnique();
        builder.HasIndex(c => c.IsActive).HasDatabaseName("ix_clients_is_active");
    }
}
