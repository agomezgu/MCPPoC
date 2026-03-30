namespace AG.MCP.Application.DTOs;

public record PaymentDto(
    Guid Id,
    Guid InvoiceId,
    string InvoiceNumber,
    decimal Amount,
    DateTime PaymentDate,
    string? Reference,
    string? Notes,
    DateTime CreatedAt);

public record CreatePaymentRequest(
    Guid InvoiceId,
    decimal Amount,
    DateTime PaymentDate,
    string? Reference,
    string? Notes);
