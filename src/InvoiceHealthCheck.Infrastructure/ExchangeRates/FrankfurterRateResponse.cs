namespace InvoiceHealthCheck.Infrastructure.ExchangeRates;

internal sealed class FrankfurterRateResponse
{
    public decimal Amount { get; set; }
    public string Base { get; set; } = default!;
    public string Date { get; set; } = default!;
    public Dictionary<string, decimal> Rates { get; set; } = new();
}

internal sealed class FrankfurterTimeSeriesResponse
{
    public decimal Amount { get; set; }
    public string Base { get; set; } = default!;
    public string StartDate { get; set; } = default!;
    public string EndDate { get; set; } = default!;
    public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; } = new();
}