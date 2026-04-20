using InvoiceHealthCheck.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceHealthCheck.Infrastructure.Persistence.Configurations;

public sealed class ContractorConfiguration : IEntityTypeConfiguration<Contractor>
{
    public void Configure(EntityTypeBuilder<Contractor> builder)
    {
        builder.ToTable("Contractors");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nip)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.CountryCode)
            .HasMaxLength(2)
            .IsFixedLength()
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.HasIndex(c => c.Nip).IsUnique();

        builder.HasMany(c => c.Invoices)
            .WithOne(i => i.Contractor)
            .HasForeignKey(i => i.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}