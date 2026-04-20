using InvoiceHealthCheck.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceHealthCheck.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.Currency)
            .HasMaxLength(3)
            .IsFixedLength()
            .IsRequired();

        builder.Property(i => i.VatRate)
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(i => i.IssueDate)
            .IsRequired();

        builder.Property(i => i.AmountInPln)
            .HasPrecision(18, 2);

        builder.Property(i => i.ExchangeRateUsed)
            .HasPrecision(18, 6);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.HasIndex(i => new { i.ContractorId, i.IssueDate });
        builder.HasIndex(i => new { i.ContractorId, i.InvoiceNumber }).IsUnique();
    }
}