using AG.MCP.Application.Common;
using AG.MCP.Application.DTOs;

namespace AG.MCP.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<PaymentDto>> GetPagedAsync(QueryParams query, CancellationToken ct = default);
    Task<PaymentDto> CreateAsync(CreatePaymentRequest request, CancellationToken ct = default);
}
