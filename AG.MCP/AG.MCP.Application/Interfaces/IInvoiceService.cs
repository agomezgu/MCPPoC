using AG.MCP.Application.Common;
using AG.MCP.Application.DTOs;

namespace AG.MCP.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<InvoiceDto>> GetPagedAsync(QueryParams query, CancellationToken ct = default);
    Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request, CancellationToken ct = default);
    Task<InvoiceDto?> UpdateAsync(Guid id, UpdateInvoiceRequest request, CancellationToken ct = default);
    Task<PagedResult<InvoiceDto>> GetPendingAsync(QueryParams query, CancellationToken ct = default);
    Task<PagedResult<InvoiceDto>> GetOverdueAsync(QueryParams query, CancellationToken ct = default);
}
