using System.Globalization;
using InvoiceHealthCheck.Domain.Enums;
using InvoiceHealthCheck.Domain.ValueObjects;

namespace InvoiceHealthCheck.Application.Anomalies.Rules;

public sealed class DuplicateDetectionRule : IAnomalyRule
{
    private const int DateWindowDays = 7;

    public string Name => nameof(DuplicateDetectionRule);

    public IEnumerable<AnomalyFlag> Evaluate(AnomalyCheckContext context)
    {
        var candidate = context.Candidate;

        foreach (var historical in context.History.Invoices)
        {
            if (!historical.Currency.Equals(candidate.Currency, StringComparison.OrdinalIgnoreCase))
                continue;

            if (historical.Amount != candidate.Amount)
                continue;

            var daysDifference = Math.Abs((candidate.IssueDate.DayNumber - historical.IssueDate.DayNumber));
            if (daysDifference > DateWindowDays)
                continue;

            if (historical.InvoiceNumber.Equals(candidate.InvoiceNumber, StringComparison.OrdinalIgnoreCase))
            {
                yield return new AnomalyFlag(
                    Name,
                    AnomalySeverity.Error,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Invoice {0} is an exact duplicate of an existing invoice (same number, amount, currency, issued on {1:yyyy-MM-dd}).",
                        candidate.InvoiceNumber,
                        historical.IssueDate));
                yield break;
            }

            yield return new AnomalyFlag(
                Name,
                AnomalySeverity.Warning,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Possible duplicate: amount {0:N2} {1} was already invoiced by the same contractor on {2:yyyy-MM-dd} as {3} (within {4}-day window).",
                    candidate.Amount,
                    candidate.Currency,
                    historical.IssueDate,
                    historical.InvoiceNumber,
                    DateWindowDays));
        }
    }
}