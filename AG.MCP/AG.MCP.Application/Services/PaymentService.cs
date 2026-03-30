using AG.MCP.Application.Common;
using AG.MCP.Application.DTOs;
using AG.MCP.Application.Interfaces;
using AG.MCP.Application.Services;
using AG.MCP.Domain.Entities;
using AG.MCP.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AG.MCP.Application.Services;

public class PaymentService(
    IPaymentRepository paymentRepository,
    IInvoiceRepository invoiceRepository,
    IUnitOfWork unitOfWork,
    ILogger<PaymentService> logger) : IPaymentService
{
    public async Task<PaymentDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var payment = await paymentRepository.GetByIdAsync(id, ct);
        return payment is null ? null : MapToDto(payment);
    }

    public async Task<PagedResult<PaymentDto>> GetPagedAsync(QueryParams query, CancellationToken ct = default)
    {
        var result = await paymentRepository.GetPagedAsync(query, ct);
        return new PagedResult<PaymentDto>(
            result.Items.Select(MapToDto).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentRequest request, CancellationToken ct = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, ct)
            ?? throw new InvalidOperationException("Invoice not found.");

        if (invoice.Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException("Cannot add payment to a cancelled invoice.");

        if (request.Amount > invoice.PendingAmount)
            throw new InvalidOperationException($"Payment amount ({request.Amount}) exceeds pending amount ({invoice.PendingAmount}).");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            InvoiceId = request.InvoiceId,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate,
            Reference = request.Reference,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await paymentRepository.AddAsync(payment, ct);

        // Update invoice amounts and status
        invoice.PaidAmount += request.Amount;
        invoice.PendingAmount = invoice.TotalAmount - invoice.PaidAmount;
        invoice.Status = InvoiceStatusCalculator.Calculate(
            invoice.TotalAmount,
            invoice.PaidAmount,
            invoice.DueDate,
            invoice.Status);
        invoice.UpdatedAt = DateTime.UtcNow;

        await invoiceRepository.UpdateAsync(invoice, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Registered payment {PaymentId} for invoice {InvoiceNumber}", payment.Id, invoice.InvoiceNumber);

        return new PaymentDto(
            payment.Id,
            payment.InvoiceId,
            invoice.InvoiceNumber,
            payment.Amount,
            payment.PaymentDate,
            payment.Reference,
            payment.Notes,
            payment.CreatedAt);
    }

    private static PaymentDto MapToDto(Payment p) => new(
        p.Id,
        p.InvoiceId,
        p.Invoice?.InvoiceNumber ?? string.Empty,
        p.Amount,
        p.PaymentDate,
        p.Reference,
        p.Notes,
        p.CreatedAt);
}
