using Refit;

namespace InvoiceHealthCheck.Infrastructure.ExchangeRates;

internal interface IFrankfurterApi
{
    [Get("/v1/latest")]
    Task<FrankfurterRateResponse> GetLatestAsync(
        [Query] string @base,
        [Query] string symbols,
        CancellationToken cancellationToken = default);

    [Get("/v1/{date}")]
    Task<FrankfurterRateResponse> GetHistoricalAsync(
        string date,
        [Query] string @base,
        [Query] string symbols,
        CancellationToken cancellationToken = default);

    [Get("/v1/{startDate}..{endDate}")]
    Task<FrankfurterTimeSeriesResponse> GetTimeSeriesAsync(
        string startDate,
        string endDate,
        [Query] string @base,
        [Query] string symbols,
        CancellationToken cancellationToken = default);
}