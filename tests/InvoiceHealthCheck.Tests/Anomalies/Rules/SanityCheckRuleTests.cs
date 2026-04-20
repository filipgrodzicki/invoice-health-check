using AwesomeAssertions;
using InvoiceHealthCheck.Application.Anomalies;
using InvoiceHealthCheck.Application.Anomalies.Rules;
using InvoiceHealthCheck.Domain.Enums;

namespace InvoiceHealthCheck.Tests.Anomalies.Rules;

public class SanityCheckRuleTests
{
    private readonly SanityCheckRule _sut = new();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void Should_flag_error_when_amount_is_zero_or_negative()
    {
        var context = BuildContext(amount: -100m, vatRate: 0.23m, issueDate: Today);

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().Contain(f => f.Severity == AnomalySeverity.Error && f.Message.Contains("positive"));
    }

    [Fact]
    public void Should_flag_warning_when_vat_rate_is_unusually_high()
    {
        var context = BuildContext(amount: 1000m, vatRate: 0.50m, issueDate: Today);

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().Contain(f => f.Severity == AnomalySeverity.Warning && f.Message.Contains("VAT"));
    }

    [Fact]
    public void Should_flag_error_when_vat_rate_is_negative()
    {
        var context = BuildContext(amount: 1000m, vatRate: -0.05m, issueDate: Today);

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().Contain(f => f.Severity == AnomalySeverity.Error && f.Message.Contains("VAT"));
    }

    [Fact]
    public void Should_flag_warning_when_issue_date_is_far_in_future()
    {
        var context = BuildContext(amount: 1000m, vatRate: 0.23m, issueDate: Today.AddDays(60));

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().Contain(f => f.Severity == AnomalySeverity.Warning && f.Message.Contains("future"));
    }

    [Fact]
    public void Should_flag_warning_when_issue_date_is_too_old()
    {
        var context = BuildContext(amount: 1000m, vatRate: 0.23m, issueDate: Today.AddDays(-365));

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().Contain(f => f.Severity == AnomalySeverity.Warning && f.Message.Contains("past"));
    }

    [Fact]
    public void Should_not_flag_when_all_values_are_reasonable()
    {
        var context = BuildContext(amount: 2500m, vatRate: 0.23m, issueDate: Today.AddDays(-5));

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().BeEmpty();
    }

    [Fact]
    public void Should_flag_multiple_issues_at_once()
    {
        var context = BuildContext(amount: -100m, vatRate: 0.50m, issueDate: Today.AddDays(60));

        var flags = _sut.Evaluate(context).ToList();

        flags.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    private static AnomalyCheckContext BuildContext(decimal amount, decimal vatRate, DateOnly issueDate)
    {
        var candidate = new InvoiceCandidate(
            ContractorNip: "DE123456789",
            InvoiceNumber: "TEST/001",
            Amount: amount,
            Currency: "EUR",
            VatRate: vatRate,
            IssueDate: issueDate);

        return new AnomalyCheckContext(
            candidate,
            new ContractorHistory("DE123456789", Array.Empty<HistoricalInvoice>()));
    }
}