using MediatR;

namespace InvoiceHealthCheck.Application.Invoices.Commands.AnalyzeInvoiceBatch;

public sealed record AnalyzeInvoiceBatchCommand(
    IReadOnlyList<InvoiceCandidateDto> Invoices)
    : IRequest<AnalyzeInvoiceBatchResult>;

public sealed record InvoiceCandidateDto(
    string ContractorNip,
    string ContractorName,
    string ContractorCountryCode,
    string InvoiceNumber,
    decimal Amount,
    string Currency,
    decimal VatRate,
    DateOnly IssueDate);