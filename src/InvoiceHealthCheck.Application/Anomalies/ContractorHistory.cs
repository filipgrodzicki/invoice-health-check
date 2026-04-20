namespace InvoiceHealthCheck.Application.Anomalies;

public sealed record HistoricalInvoice(
    string InvoiceNumber,
    decimal Amount,
    string Currency,
    DateOnly IssueDate);

public sealed record ContractorHistory(
    string Nip,
    IReadOnlyList<HistoricalInvoice> Invoices)
{
    public bool IsEmpty => Invoices.Count == 0;

    public IEnumerable<HistoricalInvoice> InCurrency(string currency) =>
        Invoices.Where(i => i.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<string> UsedCurrencies =>
        Invoices.Select(i => i.Currency.ToUpperInvariant()).Distinct().ToList();
}