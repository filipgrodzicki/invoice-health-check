using InvoiceHealthCheck.Application.Invoices.Commands.AddInvoice;
using InvoiceHealthCheck.Application.Invoices.Commands.AnalyzeInvoiceBatch;
using InvoiceHealthCheck.Application.Invoices.Queries.GetContractorStats;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceHealthCheck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Adds a new invoice, converting its amount to PLN using the exchange rate
    /// from the invoice's issue date. Creates the contractor if it does not exist.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AddInvoiceResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AddInvoiceResult>> Add(
        [FromBody] AddInvoiceCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Add), new { id = result.InvoiceId }, result);
    }

    /// <summary>
    /// Analyzes a batch of candidate invoices for anomalies without persisting them.
    /// Returns per-invoice flags (outliers, duplicates, unusual currency, sanity issues)
    /// plus a summary dashboard. Does not modify the database.
    /// </summary>
    [HttpPost("batch/analyze")]
    [ProducesResponseType(typeof(AnalyzeInvoiceBatchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AnalyzeInvoiceBatchResult>> AnalyzeBatch(
        [FromBody] AnalyzeInvoiceBatchCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns basic statistics for a contractor identified by NIP:
    /// invoice count, median/average amounts, used currencies.
    /// </summary>
    [HttpGet("contractors/{nip}/stats")]
    [ProducesResponseType(typeof(ContractorStatsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractorStatsResult>> GetStats(
        string nip,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetContractorStatsQuery(nip), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}