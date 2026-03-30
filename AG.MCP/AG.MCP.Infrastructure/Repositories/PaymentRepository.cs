using AG.MCP.Application.Common;
using AG.MCP.Application.Interfaces;
using AG.MCP.Domain.Entities;
using AG.MCP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AG.MCP.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context) => _context = context;

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Payments
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PagedResult<Payment>> GetPagedAsync(QueryParams query, CancellationToken ct = default)
    {
        var q = _context.Payments
            .Include(p => p.Invoice)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(p =>
                (p.Reference != null && p.Reference.ToLower().Contains(search)) ||
                (p.Invoice != null && p.Invoice.InvoiceNumber.ToLower().Contains(search)));
        }

        var totalCount = await q.CountAsync(ct);
        var sortBy = query.SortBy?.ToLowerInvariant() ?? "paymentdate";
        q = sortBy switch
        {
            "amount" => query.SortDescending ? q.OrderByDescending(p => p.Amount) : q.OrderBy(p => p.Amount),
            "createdat" => query.SortDescending ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt),
            _ => query.SortDescending ? q.OrderByDescending(p => p.PaymentDate) : q.OrderBy(p => p.PaymentDate)
        };

        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Payment>(items, query.Page, query.PageSize, totalCount);
    }

    public Task<Payment> AddAsync(Payment payment, CancellationToken ct = default)
    {
        _context.Payments.Add(payment);
        return Task.FromResult(payment);
    }
}
