using InvoiceHealthCheck.Application.Abstractions.ExchangeRates;
using InvoiceHealthCheck.Application.Abstractions.Persistence;
using InvoiceHealthCheck.Application.Anomalies;
using InvoiceHealthCheck.Domain.Enums;
using InvoiceHealthCheck.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceHealthCheck.Application.Invoices.Commands.AnalyzeInvoiceBatch;

public sealed class AnalyzeInvoiceBatchCommandHandler
    : IRequestHandler<AnalyzeInvoiceBatchCommand, AnalyzeInvoiceBatchResult>
{
    private const string PolishCurrencyCode = "PLN";

    private readonly IAppDbContext _db;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly IEnumerable<IAnomalyRule> _rules;

    public AnalyzeInvoiceBatchCommandHandler(
        IAppDbContext db,
        IExchangeRateService exchangeRateService,
        IEnumerable<IAnomalyRule> rules)
    {
        _db = db;
        _exchangeRateService = exchangeRateService;
        _rules = rules;
    }

    public async Task<AnalyzeInvoiceBatchResult> Handle(
        AnalyzeInvoiceBatchCommand request,
        CancellationToken cancellationToken)
    {
        var uniqueNips = request.Invoices
            .Select(i => i.ContractorNip)
            .Distinct()
            .ToList();

        var historyByNip = await LoadHistoriesAsync(uniqueNips, cancellationToken);

        var reports = new List<InvoiceAnalysisReport>();

        foreach (var dto in request.Invoices)
        {
            var candidate = new InvoiceCandidate(
                dto.ContractorNip,
                dto.InvoiceNumber,
                dto.Amount,
                dto.Currency,
                dto.VatRate,
                dto.IssueDate);

            var history = historyByNip.GetValueOrDefault(
                dto.ContractorNip,
                new ContractorHistory(dto.ContractorNip, Array.Empty<HistoricalInvoice>()));

            var context = new AnomalyCheckContext(candidate, history);

            var flags = _rules
                .SelectMany(rule => rule.Evaluate(context))
                .ToList();

            var status = DetermineStatus(flags);
            var (amountInPln, rateUsed) = status == InvoiceStatus.Invalid
                ? (null, (decimal?)null)
                : await ConvertToPlnAsync(dto, cancellationToken);

            reports.Add(new InvoiceAnalysisReport(
                dto.InvoiceNumber,
                dto.ContractorNip,
                dto.Amount,
                dto.Currency,
                amountInPln,
                rateUsed,
                status,
                flags));
        }

        return new AnalyzeInvoiceBatchResult(
            TotalInvoices: reports.Count,
            InvoicesWithErrors: reports.Count(r => r.Status == InvoiceStatus.Invalid),
            InvoicesWithWarnings: reports.Count(r => r.Status == InvoiceStatus.RequiresReview),
            CleanInvoices: reports.Count(r => r.Status == InvoiceStatus.Clean),
            Reports: reports);
    }

    private async Task<Dictionary<string, ContractorHistory>> LoadHistoriesAsync(
        IReadOnlyList<string> nips,
        CancellationToken cancellationToken)
    {
        var contractors = await _db.Contractors
            .AsNoTracking()
            .Where(c => nips.Contains(c.Nip))
            .Select(c => new
            {
                c.Id,
                c.Nip,
                Invoices = c.Invoices
                    .Select(i => new HistoricalInvoice(
                        i.InvoiceNumber,
                        i.Amount,
                        i.Currency,
                        DateOnly.FromDateTime(i.IssueDate)))
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return contractors.ToDictionary(
            c => c.Nip,
            c => new ContractorHistory(c.Nip, c.Invoices));
    }

    private async Task<(decimal? AmountInPln, decimal? Rate)> ConvertToPlnAsync(
        InvoiceCandidateDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var rate = await _exchangeRateService.GetRateAsync(
                dto.Currency,
                PolishCurrencyCode,
                dto.IssueDate,
                cancellationToken);

            var amountInPln = Math.Round(dto.Amount * rate, 2, MidpointRounding.AwayFromZero);
            return (amountInPln, rate);
        }
        catch
        {
            return (null, null);
        }
    }

    private static InvoiceStatus DetermineStatus(IReadOnlyList<AnomalyFlag> flags)
    {
        if (flags.Any(f => f.Severity == AnomalySeverity.Error))
            return InvoiceStatus.Invalid;
        if (flags.Any(f => f.Severity == AnomalySeverity.Warning))
            return InvoiceStatus.RequiresReview;
        return InvoiceStatus.Clean;
    }
}