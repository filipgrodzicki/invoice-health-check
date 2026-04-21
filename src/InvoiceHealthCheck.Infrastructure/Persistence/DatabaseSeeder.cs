using InvoiceHealthCheck.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceHealthCheck.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Contractors.AnyAsync())
            return;

        var muller = new Contractor("DE123456789", "Müller GmbH", "DE");
        var acme = new Contractor("US987654321", "ACME Inc.", "US");
        var czech = new Contractor("CZ555111222", "Moravské Pivo s.r.o.", "CZ");

        db.Contractors.AddRange(muller, acme, czech);

        var mullerInvoices = new[]
        {
            CreateInvoice(muller.Id, "MG/2026/01/001", 2400m, "EUR", 0.19m, new DateTime(2026, 1, 15), rate: 4.25m),
            CreateInvoice(muller.Id, "MG/2026/02/001", 2500m, "EUR", 0.19m, new DateTime(2026, 2, 15), rate: 4.28m),
            CreateInvoice(muller.Id, "MG/2026/03/001", 2600m, "EUR", 0.19m, new DateTime(2026, 3, 15), rate: 4.27m),
            CreateInvoice(muller.Id, "MG/2026/03/002", 2800m, "EUR", 0.19m, new DateTime(2026, 3, 28), rate: 4.26m),
        };

        var acmeInvoices = new[]
        {
            CreateInvoice(acme.Id, "ACME-2026-001", 5000m, "USD", 0.00m, new DateTime(2026, 2, 10), rate: 4.02m),
            CreateInvoice(acme.Id, "ACME-2026-002", 5500m, "USD", 0.00m, new DateTime(2026, 3, 10), rate: 4.05m),
            CreateInvoice(acme.Id, "ACME-2026-003", 5200m, "USD", 0.00m, new DateTime(2026, 4, 10), rate: 4.01m),
        };

        var czechInvoices = new[]
        {
            CreateInvoice(czech.Id, "CZ-001", 12000m, "CZK", 0.21m, new DateTime(2026, 2, 5), rate: 0.17m),
            CreateInvoice(czech.Id, "CZ-002", 13500m, "CZK", 0.21m, new DateTime(2026, 3, 5), rate: 0.17m),
            CreateInvoice(czech.Id, "CZ-003", 11500m, "CZK", 0.21m, new DateTime(2026, 4, 5), rate: 0.17m),
        };

        db.Invoices.AddRange(mullerInvoices);
        db.Invoices.AddRange(acmeInvoices);
        db.Invoices.AddRange(czechInvoices);

        await db.SaveChangesAsync();
    }

    private static Invoice CreateInvoice(
        Guid contractorId,
        string invoiceNumber,
        decimal amount,
        string currency,
        decimal vatRate,
        DateTime issueDate,
        decimal rate)
    {
        var invoice = new Invoice(contractorId, invoiceNumber, amount, currency, vatRate, issueDate);
        invoice.ApplyExchangeRate(rate);
        return invoice;
    }
}