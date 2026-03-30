using AG.MCP.Domain.Enums;

namespace AG.MCP.Application.DTOs;

public record InvoiceItemDto(
    Guid Id,
    string Description,
    string? ProductCode,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal LineTotal);

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid ClientId,
    string ClientName,
    DateTime IssueDate,
    DateTime DueDate,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal PendingAmount,
    string Status,
    string? Notes,
    DateTime CreatedAt,
    IReadOnlyList<InvoiceItemDto> Items);

public record CreateInvoiceRequest(
    Guid ClientId,
    DateTime IssueDate,
    DateTime DueDate,
    string? Notes,
    IReadOnlyList<CreateInvoiceItemRequest> Items);

public record CreateInvoiceItemRequest(
    string Description,
    string? ProductCode,
    decimal Quantity,
    string Unit,
    decimal UnitPrice);

public record UpdateInvoiceRequest(
    DateTime IssueDate,
    DateTime DueDate,
    string? Notes,
    IReadOnlyList<CreateInvoiceItemRequest> Items);
