using AG.MCP.Application.Common;
using AG.MCP.Application.DTOs;

namespace AG.Mcp.Server.InvoicingApi;

public interface IInvoicingApiClient
{
    Task<PagedResult<ClientDto>> GetClientsAsync(
        int page,
        int pageSize,
        string? search = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken ct = default);

    Task<ClientDto?> GetClientAsync(Guid id, CancellationToken ct = default);

    Task<ClientDto> CreateClientAsync(CreateClientRequest request, CancellationToken ct = default);

    Task<ClientDto?> UpdateClientAsync(Guid id, UpdateClientRequest request, CancellationToken ct = default);

    Task<ClientSummaryDto?> GetClientSummaryAsync(Guid id, CancellationToken ct = default);
}
