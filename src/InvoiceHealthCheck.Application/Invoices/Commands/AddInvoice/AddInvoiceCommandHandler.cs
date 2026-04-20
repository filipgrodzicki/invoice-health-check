using InvoiceHealthCheck.Application.Abstractions.ExchangeRates;
using InvoiceHealthCheck.Application.Abstractions.Persistence;
using InvoiceHealthCheck.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceHealthCheck.Application.Invoices.Commands.AddInvoice;

public sealed class AddInvoiceCommandHandler : IRequestHandler<AddInvoiceCommand, AddInvoiceResult>
{
    private const string PolishCurrencyCode = "PLN";

    private readonly IAppDbContext _db;
    private readonly IExchangeRateService _exchangeRateService;

    public AddInvoiceCommandHandler(
        IAppDbContext db,
        IExchangeRateService exchangeRateService)
    {
        _db = db;
        _exchangeRateService = exchangeRateService;
    }

    public async Task<AddInvoiceResult> Handle(
        AddInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var contractor = await _db.Contractors
            .FirstOrDefaultAsync(c => c.Nip == request.ContractorNip, cancellationToken);

        if (contractor is null)
        {
            contractor = new Contractor(
                request.ContractorNip,
                request.ContractorName,
                request.ContractorCountryCode);

            _db.Contractors.Add(contractor);
        }

        var invoice = new Invoice(
            contractor.Id,
            request.InvoiceNumber,
            request.Amount,
            request.Currency,
            request.VatRate,
            request.IssueDate.ToDateTime(TimeOnly.MinValue));

        var rate = await _exchangeRateService.GetRateAsync(
            request.Currency,
            PolishCurrencyCode,
            request.IssueDate,
            cancellationToken);

        invoice.ApplyExchangeRate(rate);
        _db.Invoices.Add(invoice);

        await _db.SaveChangesAsync(cancellationToken);

        return new AddInvoiceResult(
            invoice.Id,
            contractor.Id,
            invoice.AmountInPln!.Value,
            invoice.ExchangeRateUsed!.Value);
    }
}