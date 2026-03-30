using AG.MCP.Application.DTOs;
using AG.MCP.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AG.MCP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController(IReportService reportService) : ControllerBase
{
    /// <summary>
    /// Get accounts receivable summary (total pending, overdue amounts, lists of pending/overdue invoices).
    /// </summary>
    [HttpGet("accounts-receivable")]
    [ProducesResponseType(typeof(AccountsReceivableSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountsReceivableSummaryDto>> GetAccountsReceivable(CancellationToken ct = default)
    {
        var result = await reportService.GetAccountsReceivableAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Get client statement (transactions, opening/closing balance) for a date range.
    /// </summary>
    [HttpGet("clients/{id:guid}/statement")]
    [ProducesResponseType(typeof(ClientStatementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientStatementDto>> GetClientStatement(
        Guid id,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var result = await reportService.GetClientStatementAsync(id, fromDate, toDate, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Get sales summary for a date range (total sales, collected, outstanding, invoice counts).
    /// </summary>
    [HttpGet("sales-summary")]
    [ProducesResponseType(typeof(SalesSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SalesSummaryDto>> GetSalesSummary(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var result = await reportService.GetSalesSummaryAsync(fromDate, toDate, ct);
        return Ok(result);
    }
}
