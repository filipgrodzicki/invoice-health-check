using InvoiceHealthCheck.Domain.ValueObjects;

namespace InvoiceHealthCheck.Application.Invoices.Commands.AnalyzeInvoiceBatch;

public sealed record AnalyzeInvoiceBatchResult(
    int TotalInvoices,
    int InvoicesWithErrors,
    int InvoicesWithWarnings,
    int CleanInvoices,
    IReadOnlyList<InvoiceAnalysisReport> Reports);

public sealed record InvoiceAnalysisReport(
    string InvoiceNumber,
    string ContractorNip,
    decimal Amount,
    string Currency,
    decimal? AmountInPln,
    decimal? ExchangeRateUsed,
    InvoiceStatus Status,
    IReadOnlyList<AnomalyFlag> Flags);

public enum InvoiceStatus
{
    Clean = 0,
    RequiresReview = 1,
    Invalid = 2
}