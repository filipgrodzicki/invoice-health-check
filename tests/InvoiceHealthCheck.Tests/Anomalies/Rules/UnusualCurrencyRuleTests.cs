using AwesomeAssertions;
using InvoiceHealthCheck.Application.Anomalies;
using InvoiceHealthCheck.Application.Anomalies.Rules;
using InvoiceHealthCheck.Domain.Enums;

namespace InvoiceHealthCheck.Tests.Anomalies.Rules;

public class UnusualCurrencyRuleTests
{
    private readonly UnusualCurrencyRule _sut = new();

    [Fact]
    public void Should_flag_warning_when_contractor_never_used_this_currency()
    {
        var context = BuildContext(
            candidateCurrency: "USD",
            historicalCurrencies: new[] { "EUR", "EUR", "EUR", "EUR" });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().HaveCount(1);
        flags[0].Severity.Should().Be(AnomalySeverity.Warning);
        flags[0].Message.Should().Contain("USD");
        flags[0].Message.Should().Contain("EUR");
    }

    [Fact]
    public void Should_not_flag_when_currency_is_known_in_history()
    {
        var context = BuildContext(
            candidateCurrency: "EUR",
            historicalCurrencies: new[] { "EUR", "EUR", "EUR" });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().BeEmpty();
    }

    [Fact]
    public void Should_not_flag_when_history_is_too_short()
    {
        var context = BuildContext(
            candidateCurrency: "USD",
            historicalCurrencies: new[] { "EUR", "EUR" });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().BeEmpty();
    }

    [Fact]
    public void Should_not_flag_when_contractor_has_multiple_historical_currencies_including_candidate()
    {
        var context = BuildContext(
            candidateCurrency: "GBP",
            historicalCurrencies: new[] { "EUR", "USD", "GBP", "EUR" });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().BeEmpty();
    }

    [Fact]
    public void Should_be_case_insensitive_when_comparing_currencies()
    {
        var context = BuildContext(
            candidateCurrency: "eur",
            historicalCurrencies: new[] { "EUR", "EUR", "EUR" });

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().BeEmpty();
    }

    private static AnomalyCheckContext BuildContext(
        string candidateCurrency,
        string[] historicalCurrencies)
    {
        var candidate = new InvoiceCandidate(
            ContractorNip: "DE123456789",
            InvoiceNumber: "TEST/001",
            Amount: 1000m,
            Currency: candidateCurrency,
            VatRate: 0.19m,
            IssueDate: new DateOnly(2026, 4, 15));

        var historical = historicalCurrencies
            .Select((c, i) => new HistoricalInvoice($"H/{i}", 1000m, c, new DateOnly(2026, 1, 1).AddDays(i)))
            .ToArray();

        return new AnomalyCheckContext(
            candidate,
            new ContractorHistory("DE123456789", historical));
    }
}