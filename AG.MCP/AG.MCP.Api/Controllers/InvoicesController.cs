using AG.MCP.Application.Common;
using AG.MCP.Application.DTOs;
using AG.MCP.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AG.MCP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    /// <summary>
    /// Get paginated list of invoices.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<InvoiceDto>>> GetInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] Guid? clientId = null,
        CancellationToken ct = default)
    {
        var query = new QueryParams(page, pageSize, search, sortBy, sortDescending, clientId);
        var result = await invoiceService.GetPagedAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get an invoice by ID with line items.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id, CancellationToken ct = default)
    {
        var invoice = await invoiceService.GetByIdAsync(id, ct);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    /// <summary>
    /// Create a new invoice.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice([FromBody] CreateInvoiceRequest request, CancellationToken ct = default)
    {
        var invoice = await invoiceService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
    }

    /// <summary>
    /// Update an invoice (only if no payments have been recorded).
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvoiceDto>> UpdateInvoice(Guid id, [FromBody] UpdateInvoiceRequest request, CancellationToken ct = default)
    {
        var invoice = await invoiceService.UpdateAsync(id, request, ct);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    /// <summary>
    /// Get pending invoices (due date in future, pendingAmount > 0).
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(PagedResult<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<InvoiceDto>>> GetPendingInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken ct = default)
    {
        var query = new QueryParams(page, pageSize, search, sortBy, sortDescending);
        var result = await invoiceService.GetPendingAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get overdue invoices (due date past, pendingAmount > 0).
    /// </summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(PagedResult<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<InvoiceDto>>> GetOverdueInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken ct = default)
    {
        var query = new QueryParams(page, pageSize, search, sortBy, sortDescending);
        var result = await invoiceService.GetOverdueAsync(query, ct);
        return Ok(result);
    }
}
