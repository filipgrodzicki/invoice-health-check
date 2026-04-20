namespace InvoiceHealthCheck.Application.Invoices.Commands.AddInvoice;

public sealed record AddInvoiceResult(
    Guid InvoiceId,
    Guid ContractorId,
    decimal AmountInPln,
    decimal ExchangeRateUsed);