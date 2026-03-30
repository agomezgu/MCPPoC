using AG.MCP.Application.Common;
using AG.MCP.Application.DTOs;

namespace AG.MCP.Application.Interfaces;

public interface IClientService
{
    Task<ClientDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<ClientDto>> GetPagedAsync(QueryParams query, CancellationToken ct = default);
    Task<ClientDto> CreateAsync(CreateClientRequest request, CancellationToken ct = default);
    Task<ClientDto?> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken ct = default);
    Task<ClientSummaryDto?> GetSummaryAsync(Guid id, CancellationToken ct = default);
}
