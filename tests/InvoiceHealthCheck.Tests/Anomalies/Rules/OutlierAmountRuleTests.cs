using AwesomeAssertions;
using InvoiceHealthCheck.Application.Anomalies;
using InvoiceHealthCheck.Application.Anomalies.Rules;
using InvoiceHealthCheck.Domain.Enums;

namespace InvoiceHealthCheck.Tests.Anomalies.Rules;

public class OutlierAmountRuleTests
{
    private readonly OutlierAmountRule _sut = new();

    [Fact]
    public void Should_not_flag_when_history_has_less_than_three_invoices()
    {
        var context = BuildContext(
            candidateAmount: 100000m,
            historicalAmounts: new[] { 2500m, 2700m });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().BeEmpty();
    }

    [Fact]
    public void Should_flag_warning_when_amount_exceeds_three_times_median()
    {
        var context = BuildContext(
            candidateAmount: 28000m,
            historicalAmounts: new[] { 2400m, 2500m, 2600m, 2800m });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().HaveCount(1);
        flags[0].Severity.Should().Be(AnomalySeverity.Warning);
        flags[0].RuleName.Should().Be(nameof(OutlierAmountRule));
        flags[0].Message.Should().Contain("DE123456789");
        flags[0].Message.Should().Contain("median");

    }

    [Fact]
    public void Should_flag_warning_when_amount_is_less_than_third_of_median()
    {
        var context = BuildContext(
            candidateAmount: 500m,
            historicalAmounts: new[] { 2400m, 2500m, 2600m, 2800m });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().HaveCount(1);
        flags[0].Severity.Should().Be(AnomalySeverity.Warning);
    }

    [Fact]
    public void Should_not_flag_when_amount_is_within_normal_range()
    {
        var context = BuildContext(
            candidateAmount: 3000m,
            historicalAmounts: new[] { 2400m, 2500m, 2600m, 2800m });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().BeEmpty();
    }

    [Fact]
    public void Should_ignore_invoices_in_other_currencies()
    {
        var context = BuildContext(
            candidateAmount: 28000m,
            candidateCurrency: "EUR",
            historicalInvoices: new[]
            {
                new HistoricalInvoice("X/1", 100m, "USD", new DateOnly(2026, 1, 1)),
                new HistoricalInvoice("X/2", 200m, "USD", new DateOnly(2026, 2, 1)),
                new HistoricalInvoice("X/3", 300m, "USD", new DateOnly(2026, 3, 1)),
            });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().BeEmpty(
            because: "historia w USD nie powinna mieć wpływu na fakturę w EUR");
    }

    private static AnomalyCheckContext BuildContext(
        decimal candidateAmount,
        string candidateCurrency = "EUR",
        decimal[]? historicalAmounts = null,
        HistoricalInvoice[]? historicalInvoices = null)
    {
        var candidate = new InvoiceCandidate(
            ContractorNip: "DE123456789",
            InvoiceNumber: "TEST/001",
            Amount: candidateAmount,
            Currency: candidateCurrency,
            VatRate: 0.19m,
            IssueDate: new DateOnly(2026, 4, 15));

        var history = historicalInvoices
            ?? historicalAmounts?
                .Select((a, i) => new HistoricalInvoice(
                    $"HIST/{i}",
                    a,
                    candidateCurrency,
                    new DateOnly(2026, 1, 1).AddDays(i)))
                .ToArray()
            ?? Array.Empty<HistoricalInvoice>();

        return new AnomalyCheckContext(
            candidate,
            new ContractorHistory("DE123456789", history));
    }
}