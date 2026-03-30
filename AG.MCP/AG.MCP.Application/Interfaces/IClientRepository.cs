using AG.MCP.Application.Common;
using AG.MCP.Domain.Entities;

namespace AG.MCP.Application.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Client>> GetPagedAsync(QueryParams query, CancellationToken ct = default);
    Task<Client> AddAsync(Client client, CancellationToken ct = default);
    Task UpdateAsync(Client client, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> TaxIdExistsAsync(string taxId, Guid? excludeId = null, CancellationToken ct = default);
}
