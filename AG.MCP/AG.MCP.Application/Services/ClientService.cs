using AG.MCP.Application.Common;
using AG.MCP.Application.DTOs;
using AG.MCP.Application.Interfaces;
using AG.MCP.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AG.MCP.Application.Services;

public class ClientService(
    IClientRepository clientRepository,
    IInvoiceRepository invoiceRepository,
    IUnitOfWork unitOfWork,
    ILogger<ClientService> logger) : IClientService
{
    public async Task<ClientDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var client = await clientRepository.GetByIdAsync(id, ct);
        return client is null ? null : MapToDto(client);
    }

    public async Task<PagedResult<ClientDto>> GetPagedAsync(QueryParams query, CancellationToken ct = default)
    {
        var result = await clientRepository.GetPagedAsync(query, ct);
        return new PagedResult<ClientDto>(
            result.Items.Select(MapToDto).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<ClientDto> CreateAsync(CreateClientRequest request, CancellationToken ct = default)
    {
        if (await clientRepository.TaxIdExistsAsync(request.TaxId, null, ct))
            throw new InvalidOperationException($"A client with Tax ID '{request.TaxId}' already exists.");

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            TaxId = request.TaxId,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await clientRepository.AddAsync(client, ct);
        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Created client {ClientId}", client.Id);
        return MapToDto(client);
    }

    public async Task<ClientDto?> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken ct = default)
    {
        var client = await clientRepository.GetByIdAsync(id, ct);
        if (client is null) return null;

        if (await clientRepository.TaxIdExistsAsync(request.TaxId, id, ct))
            throw new InvalidOperationException($"A client with Tax ID '{request.TaxId}' already exists.");

        client.Name = request.Name;
        client.TaxId = request.TaxId;
        client.Email = request.Email;
        client.Phone = request.Phone;
        client.Address = request.Address;
        client.IsActive = request.IsActive;
        client.UpdatedAt = DateTime.UtcNow;

        await clientRepository.UpdateAsync(client, ct);
        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Updated client {ClientId}", id);
        return MapToDto(client);
    }

    public async Task<ClientSummaryDto?> GetSummaryAsync(Guid id, CancellationToken ct = default)
    {
        var client = await clientRepository.GetByIdAsync(id, ct);
        if (client is null) return null;

        var clientInvoices = await invoiceRepository.GetByClientIdAsync(id, ct);

        var today = DateTime.UtcNow.Date;
        var totalBilled = clientInvoices.Sum(i => i.TotalAmount);
        var totalPaid = clientInvoices.Sum(i => i.PaidAmount);
        var totalPending = clientInvoices.Sum(i => i.PendingAmount);
        var pendingCount = clientInvoices.Count(i => i.PendingAmount > 0 && i.DueDate.Date >= today && i.Status != Domain.Enums.InvoiceStatus.Cancelled);
        var overdueCount = clientInvoices.Count(i => i.PendingAmount > 0 && i.DueDate.Date < today);
        var totalOverdue = clientInvoices.Where(i => i.PendingAmount > 0 && i.DueDate.Date < today).Sum(i => i.PendingAmount);

        return new ClientSummaryDto(
            client.Id,
            client.Name,
            clientInvoices.Count,
            pendingCount,
            overdueCount,
            totalBilled,
            totalPaid,
            totalPending,
            totalOverdue);
    }

    private static ClientDto MapToDto(Client c) => new(
        c.Id,
        c.Name,
        c.TaxId,
        c.Email,
        c.Phone,
        c.Address,
        c.IsActive,
        c.CreatedAt);
}
