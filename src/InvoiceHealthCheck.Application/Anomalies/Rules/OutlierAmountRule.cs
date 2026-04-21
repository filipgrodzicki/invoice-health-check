using InvoiceHealthCheck.Domain.Enums;
using InvoiceHealthCheck.Domain.ValueObjects;
using System.Globalization;

namespace InvoiceHealthCheck.Application.Anomalies.Rules;

public sealed class OutlierAmountRule : IAnomalyRule
{
    private const int MinimumHistoricalInvoices = 3;
    private const decimal OutlierMultiplier = 3m;

    public string Name => nameof(OutlierAmountRule);

    public IEnumerable<AnomalyFlag> Evaluate(AnomalyCheckContext context)
    {
        if (context.Candidate.Amount <= 0m)
            yield break;

        var historical = context.History
            .InCurrency(context.Candidate.Currency)
            .Select(h => h.Amount)
            .OrderBy(a => a)
            .ToArray();

        if (historical.Length < MinimumHistoricalInvoices)
            yield break;

        var median = ComputeMedian(historical);
        var candidateAmount = context.Candidate.Amount;

        if (candidateAmount > median * OutlierMultiplier)
        {
            yield return new AnomalyFlag(
                Name,
                AnomalySeverity.Warning,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Amount {0:N2} {1} is {2:N1}x the median for contractor {3} " +
                    "(median: {4:N2} {1} based on {5} previous invoices).",
                    candidateAmount,
                    context.Candidate.Currency,
                    candidateAmount / median,
                    context.Candidate.ContractorNip,
                    median,
                    historical.Length));
        }
        else if (candidateAmount < median / OutlierMultiplier)
        {
            yield return new AnomalyFlag(
                Name,
                AnomalySeverity.Warning,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Amount {0:N2} {1} is suspiciously low, only {2:P0} of the median for contractor {3} " +
                    "(median: {4:N2} {1} based on {5} previous invoices).",
                    candidateAmount,
                    context.Candidate.Currency,
                    candidateAmount / median,
                    context.Candidate.ContractorNip,
                    median,
                    historical.Length));
        }
    }

    private static decimal ComputeMedian(decimal[] sortedValues)
    {
        var mid = sortedValues.Length / 2;
        return sortedValues.Length % 2 == 0
            ? (sortedValues[mid - 1] + sortedValues[mid]) / 2m
            : sortedValues[mid];
    }
}