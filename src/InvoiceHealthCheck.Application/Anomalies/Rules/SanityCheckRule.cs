using System.Globalization;
using InvoiceHealthCheck.Domain.Enums;
using InvoiceHealthCheck.Domain.ValueObjects;

namespace InvoiceHealthCheck.Application.Anomalies.Rules;

public sealed class SanityCheckRule : IAnomalyRule
{
    private const int MaxFutureDays = 30;
    private const int MaxPastDays = 180;
    private const decimal MaxReasonableVatRate = 0.30m;

    public string Name => nameof(SanityCheckRule);

    public IEnumerable<AnomalyFlag> Evaluate(AnomalyCheckContext context)
    {
        var candidate = context.Candidate;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (candidate.Amount <= 0m)
        {
            yield return new AnomalyFlag(
                Name,
                AnomalySeverity.Error,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Invoice amount must be positive, got {0:N2} {1}.",
                    candidate.Amount,
                    candidate.Currency));
        }

        if (candidate.VatRate < 0m)
        {
            yield return new AnomalyFlag(
                Name,
                AnomalySeverity.Error,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "VAT rate cannot be negative, got {0:P2}.",
                    candidate.VatRate));
        }
        else if (candidate.VatRate > MaxReasonableVatRate)
        {
            yield return new AnomalyFlag(
                Name,
                AnomalySeverity.Warning,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "VAT rate {0:P2} is unusually high, typical EU rates are 0-25%. Possible data entry error.",
                    candidate.VatRate));
        }

        var daysFromToday = candidate.IssueDate.DayNumber - today.DayNumber;

        if (daysFromToday > MaxFutureDays)
        {
            yield return new AnomalyFlag(
                Name,
                AnomalySeverity.Warning,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Issue date {0:yyyy-MM-dd} is {1} days in the future, please verify.",
                    candidate.IssueDate,
                    daysFromToday));
        }
        else if (daysFromToday < -MaxPastDays)
        {
            yield return new AnomalyFlag(
                Name,
                AnomalySeverity.Warning,
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Issue date {0:yyyy-MM-dd} is {1} days in the past, may be past VAT deduction period.",
                    candidate.IssueDate,
                    Math.Abs(daysFromToday)));
        }
    }
}