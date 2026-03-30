using AG.MCP.Application.Common;
using AG.MCP.Application.DTOs;
using AG.MCP.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AG.MCP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    /// <summary>
    /// Get paginated list of clients with optional search and sorting.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ClientDto>>> GetClients(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken ct = default)
    {
        var query = new QueryParams(page, pageSize, search, sortBy, sortDescending);
        var result = await clientService.GetPagedAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get a client by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> GetClient(Guid id, CancellationToken ct = default)
    {
        var client = await clientService.GetByIdAsync(id, ct);
        return client is null ? NotFound() : Ok(client);
    }

    /// <summary>
    /// Create a new client.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientDto>> CreateClient([FromBody] CreateClientRequest request, CancellationToken ct = default)
    {
        var client = await clientService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
    }

    /// <summary>
    /// Update an existing client.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientDto>> UpdateClient(Guid id, [FromBody] UpdateClientRequest request, CancellationToken ct = default)
    {
        var client = await clientService.UpdateAsync(id, request, ct);
        return client is null ? NotFound() : Ok(client);
    }

    /// <summary>
    /// Get financial summary for a client (invoices, payments, pending/overdue amounts).
    /// </summary>
    [HttpGet("{id:guid}/summary")]
    [ProducesResponseType(typeof(ClientSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientSummaryDto>> GetClientSummary(Guid id, CancellationToken ct = default)
    {
        var summary = await clientService.GetSummaryAsync(id, ct);
        return summary is null ? NotFound() : Ok(summary);
    }
}
