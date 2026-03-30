using AG.MCP.Application.Common;
using AG.MCP.Application.DTOs;
using AG.MCP.Application.Interfaces;
using AG.MCP.Application.Services;
using AG.MCP.Domain.Entities;
using AG.MCP.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AG.MCP.Application.Services;

public class InvoiceService(
    IInvoiceRepository invoiceRepository,
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ILogger<InvoiceService> logger) : IInvoiceService
{
    public async Task<InvoiceDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await invoiceRepository.GetByIdWithDetailsAsync(id, ct);
        return invoice is null ? null : MapToDto(invoice);
    }

    public async Task<PagedResult<InvoiceDto>> GetPagedAsync(QueryParams query, CancellationToken ct = default)
    {
        var result = await invoiceRepository.GetPagedAsync(query, ct);
        return new PagedResult<InvoiceDto>(
            result.Items.Select(MapToDto).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request, CancellationToken ct = default)
    {
        var client = await clientRepository.GetByIdAsync(request.ClientId, ct)
            ?? throw new InvalidOperationException("Client not found.");

        var invoiceNumber = await invoiceRepository.GetNextInvoiceNumberAsync(ct);
        var totalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            ClientId = request.ClientId,
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            TotalAmount = totalAmount,
            PaidAmount = 0,
            PendingAmount = totalAmount,
            Status = InvoiceStatusCalculator.Calculate(totalAmount, 0, request.DueDate),
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            Items = request.Items.Select((item, idx) => new InvoiceItem
            {
                Id = Guid.NewGuid(),
                InvoiceId = Guid.Empty, // Set after invoice is created
                Description = item.Description,
                ProductCode = item.ProductCode,
                Quantity = item.Quantity,
                Unit = item.Unit,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        foreach (var item in invoice.Items)
            item.InvoiceId = invoice.Id;

        await invoiceRepository.AddAsync(invoice, ct);
        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Created invoice {InvoiceNumber}", invoice.InvoiceNumber);
        return MapToDto(invoice);
    }

    public async Task<InvoiceDto?> UpdateAsync(Guid id, UpdateInvoiceRequest request, CancellationToken ct = default)
    {
        var invoice = await invoiceRepository.GetByIdWithDetailsAsync(id, ct);
        if (invoice is null) return null;

        if (invoice.Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException("Cannot update a cancelled invoice.");

        if (invoice.PaidAmount > 0)
            throw new InvalidOperationException("Cannot update an invoice that has payments. Create a credit note instead.");

        var totalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice);

        invoice.IssueDate = request.IssueDate;
        invoice.DueDate = request.DueDate;
        invoice.TotalAmount = totalAmount;
        invoice.PendingAmount = totalAmount;
        invoice.Notes = request.Notes;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.Status = InvoiceStatusCalculator.Calculate(totalAmount, invoice.PaidAmount, request.DueDate);

        invoice.Items.Clear();
        foreach (var item in request.Items)
        {
            invoice.Items.Add(new InvoiceItem
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                Description = item.Description,
                ProductCode = item.ProductCode,
                Quantity = item.Quantity,
                Unit = item.Unit,
                UnitPrice = item.UnitPrice
            });
        }

        await invoiceRepository.UpdateAsync(invoice, ct);
        await unitOfWork.SaveChangesAsync(ct);
        logger.LogInformation("Updated invoice {InvoiceId}", id);
        return MapToDto(invoice);
    }

    public async Task<PagedResult<InvoiceDto>> GetPendingAsync(QueryParams query, CancellationToken ct = default)
    {
        var result = await invoiceRepository.GetPendingAsync(query, ct);
        return new PagedResult<InvoiceDto>(
            result.Items.Select(MapToDto).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<PagedResult<InvoiceDto>> GetOverdueAsync(QueryParams query, CancellationToken ct = default)
    {
        var result = await invoiceRepository.GetOverdueAsync(query, ct);
        return new PagedResult<InvoiceDto>(
            result.Items.Select(MapToDto).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    private static InvoiceDto MapToDto(Invoice i) => new(
        i.Id,
        i.InvoiceNumber,
        i.ClientId,
        i.Client?.Name ?? string.Empty,
        i.IssueDate,
        i.DueDate,
        i.TotalAmount,
        i.PaidAmount,
        i.PendingAmount,
        i.Status.ToString(),
        i.Notes,
        i.CreatedAt,
        i.Items.Select(item => new InvoiceItemDto(
            item.Id,
            item.Description,
            item.ProductCode,
            item.Quantity,
            item.Unit,
            item.UnitPrice,
            item.Quantity * item.UnitPrice)).ToList());
}
