namespace AG.Mcp.Server.InvoicingApi.Models;

public record ClientDto(
    Guid Id,
    string Name,
    string TaxId,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive,
    DateTime CreatedAt);

public record CreateClientRequest(
    string Name,
    string TaxId,
    string? Email,
    string? Phone,
    string? Address);

public record UpdateClientRequest(
    string Name,
    string TaxId,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive);

public record ClientSummaryDto(
    Guid ClientId,
    string ClientName,
    int TotalInvoices,
    int PendingInvoices,
    int OverdueInvoices,
    decimal TotalBilled,
    decimal TotalPaid,
    decimal TotalPending,
    decimal TotalOverdue);
