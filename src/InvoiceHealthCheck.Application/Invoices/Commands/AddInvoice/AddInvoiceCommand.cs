using MediatR;

namespace InvoiceHealthCheck.Application.Invoices.Commands.AddInvoice;

public sealed record AddInvoiceCommand(
    string ContractorNip,
    string ContractorName,
    string ContractorCountryCode,
    string InvoiceNumber,
    decimal Amount,
    string Currency,
    decimal VatRate,
    DateOnly IssueDate) : IRequest<AddInvoiceResult>;