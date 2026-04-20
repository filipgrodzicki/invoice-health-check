namespace InvoiceHealthCheck.Application.Anomalies;

public sealed record InvoiceCandidate(
    string ContractorNip,
    string InvoiceNumber,
    decimal Amount,
    string Currency,
    decimal VatRate,
    DateOnly IssueDate);