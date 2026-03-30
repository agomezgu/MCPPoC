using AG.MCP.Application.Common;
using AG.MCP.Application.Interfaces;
using AG.MCP.Domain.Entities;
using AG.MCP.Domain.Enums;
using AG.MCP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AG.MCP.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _context;

    public InvoiceRepository(AppDbContext context) => _context = context;

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Invoices
            .Include(i => i.Client)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<Invoice?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        await _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<Invoice>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default) =>
        await _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Payments)
            .Where(i => i.ClientId == clientId)
            .OrderBy(i => i.IssueDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Invoice>> GetAllWithPaymentsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var q = _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Payments)
            .AsNoTracking()
            .Where(i => i.Status != InvoiceStatus.Cancelled);

        if (fromDate.HasValue)
            q = q.Where(i => i.IssueDate.Date >= fromDate.Value.Date);
        if (toDate.HasValue)
            q = q.Where(i => i.IssueDate.Date <= toDate.Value.Date);

        return await q.OrderBy(i => i.IssueDate).ToListAsync(ct);
    }

    public async Task<PagedResult<Invoice>> GetPagedAsync(QueryParams query, CancellationToken ct = default)
    {
        var q = _context.Invoices
            .Include(i => i.Client)
            .AsNoTracking()
            .AsQueryable();

        if (query.ClientId.HasValue)
            q = q.Where(i => i.ClientId == query.ClientId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(i =>
                i.InvoiceNumber.ToLower().Contains(search) ||
                (i.Client != null && i.Client.Name.ToLower().Contains(search)));
        }

        var totalCount = await q.CountAsync(ct);

        var sortBy = query.SortBy?.ToLowerInvariant() ?? "issuedate";
        q = sortBy switch
        {
            "invoicenumber" => query.SortDescending ? q.OrderByDescending(i => i.InvoiceNumber) : q.OrderBy(i => i.InvoiceNumber),
            "duedate" => query.SortDescending ? q.OrderByDescending(i => i.DueDate) : q.OrderBy(i => i.DueDate),
            "totalamount" => query.SortDescending ? q.OrderByDescending(i => i.TotalAmount) : q.OrderBy(i => i.TotalAmount),
            "status" => query.SortDescending ? q.OrderByDescending(i => i.Status) : q.OrderBy(i => i.Status),
            _ => query.SortDescending ? q.OrderByDescending(i => i.IssueDate) : q.OrderBy(i => i.IssueDate)
        };

        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Invoice>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<PagedResult<Invoice>> GetPendingAsync(QueryParams query, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var q = _context.Invoices
            .Include(i => i.Client)
            .AsNoTracking()
            .Where(i => i.PendingAmount > 0 && i.Status != InvoiceStatus.Cancelled);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(i =>
                i.InvoiceNumber.ToLower().Contains(search) ||
                (i.Client != null && i.Client.Name.ToLower().Contains(search)));
        }

        var totalCount = await q.CountAsync(ct);
        var sortBy = query.SortBy?.ToLowerInvariant() ?? "duedate";
        q = sortBy switch
        {
            "invoicenumber" => query.SortDescending ? q.OrderByDescending(i => i.InvoiceNumber) : q.OrderBy(i => i.InvoiceNumber),
            "pendingamount" => query.SortDescending ? q.OrderByDescending(i => i.PendingAmount) : q.OrderBy(i => i.PendingAmount),
            _ => query.SortDescending ? q.OrderByDescending(i => i.DueDate) : q.OrderBy(i => i.DueDate)
        };

        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Invoice>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<PagedResult<Invoice>> GetOverdueAsync(QueryParams query, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var q = _context.Invoices
            .Include(i => i.Client)
            .AsNoTracking()
            .Where(i => i.PendingAmount > 0 && i.DueDate.Date < today && i.Status != InvoiceStatus.Cancelled);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(i =>
                i.InvoiceNumber.ToLower().Contains(search) ||
                (i.Client != null && i.Client.Name.ToLower().Contains(search)));
        }

        var totalCount = await q.CountAsync(ct);
        var sortBy = query.SortBy?.ToLowerInvariant() ?? "duedate";
        q = query.SortDescending ? q.OrderByDescending(i => i.DueDate) : q.OrderBy(i => i.DueDate);

        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Invoice>(items, query.Page, query.PageSize, totalCount);
    }

    public Task<Invoice> AddAsync(Invoice invoice, CancellationToken ct = default)
    {
        _context.Invoices.Add(invoice);
        return Task.FromResult(invoice);
    }

    public Task UpdateAsync(Invoice invoice, CancellationToken ct = default)
    {
        _context.Invoices.Update(invoice);
        return Task.CompletedTask;
    }

    public async Task<string> GetNextInvoiceNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var lastNumber = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith($"INV-{year}-"))
            .OrderByDescending(i => i.InvoiceNumber)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(ct);

        var nextSeq = 1;
        if (!string.IsNullOrEmpty(lastNumber))
        {
            var parts = lastNumber.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out var seq))
                nextSeq = seq + 1;
        }

        return $"INV-{year}-{nextSeq:D5}";
    }
}
