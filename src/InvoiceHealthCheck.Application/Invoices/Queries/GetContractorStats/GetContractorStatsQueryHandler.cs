using InvoiceHealthCheck.Application.Abstractions.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceHealthCheck.Application.Invoices.Queries.GetContractorStats;

public sealed class GetContractorStatsQueryHandler
    : IRequestHandler<GetContractorStatsQuery, ContractorStatsResult?>
{
    private readonly IAppDbContext _db;

    public GetContractorStatsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<ContractorStatsResult?> Handle(
        GetContractorStatsQuery request,
        CancellationToken cancellationToken)
    {
        var contractor = await _db.Contractors
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Nip == request.Nip, cancellationToken);

        if (contractor is null)
            return null;

        var invoices = await _db.Invoices
            .AsNoTracking()
            .Where(i => i.ContractorId == contractor.Id)
            .Select(i => new { i.Amount, i.Currency })
            .ToListAsync(cancellationToken);

        if (invoices.Count == 0)
        {
            return new ContractorStatsResult(
                contractor.Id,
                contractor.Nip,
                contractor.Name,
                0,
                MedianAmount: null,
                AverageAmount: null,
                UsedCurrencies: Array.Empty<string>());
        }

        var amounts = invoices.Select(i => i.Amount).OrderBy(a => a).ToArray();
        var median = ComputeMedian(amounts);
        var average = amounts.Average();
        var currencies = invoices.Select(i => i.Currency).Distinct().OrderBy(c => c).ToList();

        return new ContractorStatsResult(
            contractor.Id,
            contractor.Nip,
            contractor.Name,
            invoices.Count,
            median,
            average,
            currencies);
    }

    private static decimal ComputeMedian(decimal[] sortedValues)
    {
        var count = sortedValues.Length;
        if (count == 0)
            throw new InvalidOperationException("Cannot compute median of empty array.");

        var mid = count / 2;
        return count % 2 == 0
            ? (sortedValues[mid - 1] + sortedValues[mid]) / 2m
            : sortedValues[mid];
    }
}