namespace InvoiceHealthCheck.Application.Invoices.Queries.GetContractorStats;

public sealed record ContractorStatsResult(
    Guid ContractorId,
    string Nip,
    string Name,
    int InvoiceCount,
    decimal? MedianAmount,
    decimal? AverageAmount,
    IReadOnlyList<string> UsedCurrencies);