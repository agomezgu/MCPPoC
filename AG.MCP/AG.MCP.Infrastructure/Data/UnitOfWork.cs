using AG.MCP.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AG.MCP.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);

    public void Dispose() => _context.Dispose();
}
