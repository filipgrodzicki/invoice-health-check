using InvoiceHealthCheck.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace InvoiceHealthCheck.Application.Abstractions.Persistence;

public interface IAppDbContext
{
    DbSet<Contractor> Contractors { get; }
    DbSet<Invoice> Invoices { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}