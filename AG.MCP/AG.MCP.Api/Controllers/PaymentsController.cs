using AG.MCP.Application.Common;
using AG.MCP.Application.DTOs;
using AG.MCP.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AG.MCP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    /// <summary>
    /// Get paginated list of payments.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PaymentDto>>> GetPayments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken ct = default)
    {
        var query = new QueryParams(page, pageSize, search, sortBy, sortDescending);
        var result = await paymentService.GetPagedAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get a payment by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetPayment(Guid id, CancellationToken ct = default)
    {
        var payment = await paymentService.GetByIdAsync(id, ct);
        return payment is null ? NotFound() : Ok(payment);
    }

    /// <summary>
    /// Register a payment for an invoice. Automatically updates invoice paidAmount, pendingAmount, and status.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentRequest request, CancellationToken ct = default)
    {
        var payment = await paymentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
    }
}
