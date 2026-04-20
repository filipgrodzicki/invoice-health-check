using AwesomeAssertions;
using InvoiceHealthCheck.Application.Anomalies;
using InvoiceHealthCheck.Application.Anomalies.Rules;
using InvoiceHealthCheck.Domain.Enums;

namespace InvoiceHealthCheck.Tests.Anomalies.Rules;

public class DuplicateDetectionRuleTests
{
    private readonly DuplicateDetectionRule _sut = new();

    [Fact]
    public void Should_flag_error_when_invoice_number_amount_currency_and_date_match()
    {
        var context = BuildContext(
            candidateNumber: "MG/2026/04/087",
            candidateAmount: 2500m,
            candidateCurrency: "EUR",
            candidateDate: new DateOnly(2026, 4, 15),
            historical: new[]
            {
                new HistoricalInvoice("MG/2026/04/087", 2500m, "EUR", new DateOnly(2026, 4, 15))
            });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().HaveCount(1);
        flags[0].Severity.Should().Be(AnomalySeverity.Error);
        flags[0].Message.Should().Contain("exact duplicate");
    }

    [Fact]
    public void Should_flag_warning_when_amount_currency_match_within_date_window_but_number_differs()
    {
        var context = BuildContext(
            candidateNumber: "MG/2026/04/088",
            candidateAmount: 2500m,
            candidateCurrency: "EUR",
            candidateDate: new DateOnly(2026, 4, 15),
            historical: new[]
            {
                new HistoricalInvoice("MG/2026/04/087", 2500m, "EUR", new DateOnly(2026, 4, 12))
            });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().HaveCount(1);
        flags[0].Severity.Should().Be(AnomalySeverity.Warning);
        flags[0].Message.Should().Contain("Possible duplicate");
    }

    [Fact]
    public void Should_not_flag_when_date_is_outside_window()
    {
        var context = BuildContext(
            candidateNumber: "MG/2026/04/088",
            candidateAmount: 2500m,
            candidateCurrency: "EUR",
            candidateDate: new DateOnly(2026, 4, 15),
            historical: new[]
            {
                new HistoricalInvoice("MG/2026/04/087", 2500m, "EUR", new DateOnly(2026, 4, 1))
            });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().BeEmpty();
    }

    [Fact]
    public void Should_not_flag_when_amount_differs()
    {
        var context = BuildContext(
            candidateNumber: "MG/2026/04/088",
            candidateAmount: 2500m,
            candidateCurrency: "EUR",
            candidateDate: new DateOnly(2026, 4, 15),
            historical: new[]
            {
                new HistoricalInvoice("MG/2026/04/087", 2501m, "EUR", new DateOnly(2026, 4, 14))
            });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().BeEmpty();
    }

    [Fact]
    public void Should_not_flag_when_currency_differs()
    {
        var context = BuildContext(
            candidateNumber: "MG/2026/04/088",
            candidateAmount: 2500m,
            candidateCurrency: "EUR",
            candidateDate: new DateOnly(2026, 4, 15),
            historical: new[]
            {
                new HistoricalInvoice("MG/2026/04/087", 2500m, "USD", new DateOnly(2026, 4, 14))
            });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().BeEmpty();
    }

    private static AnomalyCheckContext BuildContext(
        string candidateNumber,
        decimal candidateAmount,
        string candidateCurrency,
        DateOnly candidateDate,
        HistoricalInvoice[] historical)
    {
        var candidate = new InvoiceCandidate(
            ContractorNip: "DE123456789",
            InvoiceNumber: candidateNumber,
            Amount: candidateAmount,
            Currency: candidateCurrency,
            VatRate: 0.19m,
            IssueDate: candidateDate);

        return new AnomalyCheckContext(
            candidate,
            new ContractorHistory("DE123456789", historical));
    }
}