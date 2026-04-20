using System.Globalization;
using InvoiceHealthCheck.Domain.Enums;
using InvoiceHealthCheck.Domain.ValueObjects;

namespace InvoiceHealthCheck.Application.Anomalies.Rules;

public sealed class UnusualCurrencyRule : IAnomalyRule
{
    private const int MinimumHistoricalInvoices = 3;

    public string Name => nameof(UnusualCurrencyRule);

    public IEnumerable<AnomalyFlag> Evaluate(AnomalyCheckContext context)
    {
        if (context.History.Invoices.Count < MinimumHistoricalInvoices)
            yield break;

        var candidateCurrency = context.Candidate.Currency.ToUpperInvariant();
        var usedCurrencies = context.History.UsedCurrencies;

        if (usedCurrencies.Contains(candidateCurrency))
            yield break;

        yield return new AnomalyFlag(
            Name,
            AnomalySeverity.Warning,
            string.Format(
                CultureInfo.InvariantCulture,
                "Contractor {0} has never issued invoices in {1} before (previously used: {2}). " +
                "Verify the currency or check if this could be fraudulent activity.",
                context.Candidate.ContractorNip,
                candidateCurrency,
                string.Join(", ", usedCurrencies)));
    }
}