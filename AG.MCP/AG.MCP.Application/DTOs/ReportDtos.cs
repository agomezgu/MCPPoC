namespace AG.MCP.Application.DTOs;

public record AccountsReceivableSummaryDto(
    decimal TotalReceivable,
    decimal TotalOverdue,
    int TotalPendingInvoices,
    int TotalOverdueInvoices,
    IReadOnlyList<InvoiceSummaryItemDto> PendingInvoices,
    IReadOnlyList<InvoiceSummaryItemDto> OverdueInvoices);

public record InvoiceSummaryItemDto(
    Guid InvoiceId,
    string InvoiceNumber,
    string ClientName,
    DateTime DueDate,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal PendingAmount,
    int DaysOverdue);

public record ClientStatementDto(
    Guid ClientId,
    string ClientName,
    DateTime FromDate,
    DateTime ToDate,
    decimal OpeningBalance,
    decimal TotalInvoiced,
    decimal TotalPaid,
    decimal ClosingBalance,
    IReadOnlyList<StatementTransactionDto> Transactions);

public record StatementTransactionDto(
    DateTime Date,
    string Type,  // "Invoice" or "Payment"
    string Reference,
    decimal Debit,
    decimal Credit,
    decimal Balance);

public record SalesSummaryDto(
    DateTime FromDate,
    DateTime ToDate,
    decimal TotalSales,
    decimal TotalCollected,
    decimal TotalOutstanding,
    int InvoiceCount,
    int PaidInvoiceCount,
    int PendingInvoiceCount);
