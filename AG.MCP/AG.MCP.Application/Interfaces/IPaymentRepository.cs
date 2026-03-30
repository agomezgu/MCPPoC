using AG.MCP.Application.Common;
using AG.MCP.Domain.Entities;

namespace AG.MCP.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Payment>> GetPagedAsync(QueryParams query, CancellationToken ct = default);
    Task<Payment> AddAsync(Payment payment, CancellationToken ct = default);
}
