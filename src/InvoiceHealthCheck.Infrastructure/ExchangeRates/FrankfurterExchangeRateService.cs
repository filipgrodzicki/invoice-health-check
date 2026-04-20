using System.Globalization;
using InvoiceHealthCheck.Application.Abstractions.ExchangeRates;

namespace InvoiceHealthCheck.Infrastructure.ExchangeRates;

internal sealed class FrankfurterExchangeRateService : IExchangeRateService
{
    private readonly IFrankfurterApi _api;

    public FrankfurterExchangeRateService(IFrankfurterApi api)
    {
        _api = api;
    }

    public async Task<decimal> GetRateAsync(
        string fromCurrency,
        string toCurrency,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        ValidateCurrencies(fromCurrency, toCurrency);

        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
            return 1m;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var response = date >= today
            ? await _api.GetLatestAsync(fromCurrency.ToUpperInvariant(), toCurrency.ToUpperInvariant(), cancellationToken)
            : await _api.GetHistoricalAsync(
                date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                fromCurrency.ToUpperInvariant(),
                toCurrency.ToUpperInvariant(),
                cancellationToken);

        if (!response.Rates.TryGetValue(toCurrency.ToUpperInvariant(), out var rate))
            throw new InvalidOperationException(
                $"Exchange rate for {toCurrency} not returned by Frankfurter API.");

        return rate;
    }

    public async Task<IReadOnlyList<DailyRate>> GetRatesInRangeAsync(
        string fromCurrency,
        string toCurrency,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        ValidateCurrencies(fromCurrency, toCurrency);

        if (startDate > endDate)
            throw new ArgumentException("Start date must be before or equal to end date.");

        var response = await _api.GetTimeSeriesAsync(
            startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            endDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            fromCurrency.ToUpperInvariant(),
            toCurrency.ToUpperInvariant(),
            cancellationToken);

        var result = new List<DailyRate>();
        foreach (var (dateString, rates) in response.Rates)
        {
            if (rates.TryGetValue(toCurrency.ToUpperInvariant(), out var rate)
                && DateOnly.TryParse(dateString, CultureInfo.InvariantCulture, out var parsedDate))
            {
                result.Add(new DailyRate(parsedDate, rate));
            }
        }

        return result.OrderBy(r => r.Date).ToList();
    }

    private static void ValidateCurrencies(string fromCurrency, string toCurrency)
    {
        if (string.IsNullOrWhiteSpace(fromCurrency) || fromCurrency.Length != 3)
            throw new ArgumentException("From currency must be 3 letters (ISO 4217).", nameof(fromCurrency));
        if (string.IsNullOrWhiteSpace(toCurrency) || toCurrency.Length != 3)
            throw new ArgumentException("To currency must be 3 letters (ISO 4217).", nameof(toCurrency));
    }
}