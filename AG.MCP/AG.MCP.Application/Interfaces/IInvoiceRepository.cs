using AG.MCP.Application.Common;
using AG.MCP.Domain.Entities;

namespace AG.MCP.Application.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Invoice?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> GetAllWithPaymentsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task<PagedResult<Invoice>> GetPagedAsync(QueryParams query, CancellationToken ct = default);
    Task<PagedResult<Invoice>> GetPendingAsync(QueryParams query, CancellationToken ct = default);
    Task<PagedResult<Invoice>> GetOverdueAsync(QueryParams query, CancellationToken ct = default);
    Task<Invoice> AddAsync(Invoice invoice, CancellationToken ct = default);
    Task UpdateAsync(Invoice invoice, CancellationToken ct = default);
    Task<string> GetNextInvoiceNumberAsync(CancellationToken ct = default);
}
