using MediatR;

namespace InvoiceHealthCheck.Application.Invoices.Queries.GetContractorStats;

public sealed record GetContractorStatsQuery(string Nip)
    : IRequest<ContractorStatsResult?>;