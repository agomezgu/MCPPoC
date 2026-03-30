using AG.MCP.Application.Common;
using AG.MCP.Application.Interfaces;
using AG.MCP.Domain.Entities;
using AG.MCP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AG.MCP.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;

    public ClientRepository(AppDbContext context) => _context = context;

    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Clients.FindAsync([id], ct);

    public async Task<PagedResult<Client>> GetPagedAsync(QueryParams query, CancellationToken ct = default)
    {
        var q = _context.Clients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(c =>
                c.Name.ToLower().Contains(search) ||
                (c.TaxId != null && c.TaxId.ToLower().Contains(search)) ||
                (c.Email != null && c.Email.ToLower().Contains(search)));
        }

        var totalCount = await q.CountAsync(ct);

        var sortBy = query.SortBy?.ToLowerInvariant() ?? "name";
        q = sortBy switch
        {
            "taxid" => query.SortDescending ? q.OrderByDescending(c => c.TaxId) : q.OrderBy(c => c.TaxId),
            "createdat" => query.SortDescending ? q.OrderByDescending(c => c.CreatedAt) : q.OrderBy(c => c.CreatedAt),
            _ => query.SortDescending ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name)
        };

        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Client>(items, query.Page, query.PageSize, totalCount);
    }

    public Task<Client> AddAsync(Client client, CancellationToken ct = default)
    {
        _context.Clients.Add(client);
        return Task.FromResult(client);
    }

    public Task UpdateAsync(Client client, CancellationToken ct = default)
    {
        _context.Clients.Update(client);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        await _context.Clients.AnyAsync(c => c.Id == id, ct);

    public async Task<bool> TaxIdExistsAsync(string taxId, Guid? excludeId, CancellationToken ct = default)
    {
        var q = _context.Clients.Where(c => c.TaxId == taxId);
        if (excludeId.HasValue)
            q = q.Where(c => c.Id != excludeId.Value);
        return await q.AnyAsync(ct);
    }
}
