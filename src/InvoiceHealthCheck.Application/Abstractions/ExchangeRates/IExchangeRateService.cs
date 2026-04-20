namespace InvoiceHealthCheck.Application.Abstractions.ExchangeRates;

public interface IExchangeRateService
{
    Task<decimal> GetRateAsync(
        string fromCurrency,
        string toCurrency,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyRate>> GetRatesInRangeAsync(
        string fromCurrency,
        string toCurrency,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);
}

public sealed record DailyRate(DateOnly Date, decimal Rate);