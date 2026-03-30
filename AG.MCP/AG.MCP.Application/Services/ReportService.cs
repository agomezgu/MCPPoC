using AG.MCP.Application.Common;
using AG.MCP.Application.DTOs;
using AG.MCP.Application.Interfaces;
using AG.MCP.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AG.MCP.Application.Services;

public class ReportService(
    IInvoiceRepository invoiceRepository,
    IClientRepository clientRepository,
    ILogger<ReportService> logger) : IReportService
{
    public async Task<AccountsReceivableSummaryDto> GetAccountsReceivableAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var query = new Application.Common.QueryParams(1, 10000);
        var invoiceResult = await invoiceRepository.GetPendingAsync(query, ct);
        var allPending = invoiceResult.Items.ToList();

        var overdue = allPending.Where(i => i.DueDate.Date < today && i.PendingAmount > 0).ToList();
        var pending = allPending.Where(i => i.DueDate.Date >= today && i.PendingAmount > 0).ToList();

        var totalReceivable = allPending.Sum(i => i.PendingAmount);
        var totalOverdue = overdue.Sum(i => i.PendingAmount);

        return new AccountsReceivableSummaryDto(
            totalReceivable,
            totalOverdue,
            pending.Count,
            overdue.Count,
            pending.Select(i => ToSummaryItem(i, today)).ToList(),
            overdue.Select(i => ToSummaryItem(i, today)).ToList());
    }

    public async Task<ClientStatementDto?> GetClientStatementAsync(Guid clientId, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var client = await clientRepository.GetByIdAsync(clientId, ct);
        if (client is null) return null;

        var to = toDate ?? DateTime.UtcNow.Date;
        var from = fromDate ?? to.AddMonths(-1);

        var clientInvoices = await invoiceRepository.GetByClientIdAsync(clientId, ct);
        var filtered = clientInvoices
            .Where(i => i.IssueDate.Date <= to)
            .OrderBy(i => i.IssueDate)
            .ToList();

        var payments = filtered.SelectMany(i => i.Payments).ToList();

        var transactions = new List<StatementTransactionDto>();
        decimal runningBalance = 0;

        // Opening balance (invoices before fromDate)
        var openingInvoices = filtered.Where(i => i.IssueDate.Date < from).ToList();
        var openingPaid = openingInvoices.Sum(i => i.PaidAmount);
        var openingInvoiced = openingInvoices.Sum(i => i.TotalAmount);
        var openingPending = openingInvoiced - openingPaid;
        runningBalance = openingPending;

        var periodInvoices = filtered.Where(i => i.IssueDate.Date >= from && i.IssueDate.Date <= to).ToList();
        var periodPayments = payments.Where(p => p.PaymentDate.Date >= from && p.PaymentDate.Date <= to).ToList();

        var allEvents = periodInvoices
            .Select(i => (Date: i.IssueDate, Type: "Invoice", Ref: i.InvoiceNumber, Debit: i.TotalAmount, Credit: 0m))
            .Concat(periodPayments.Select(p => (Date: p.PaymentDate, Type: "Payment", Ref: p.Reference ?? p.Id.ToString("N")[..8], Debit: 0m, Credit: p.Amount)))
            .OrderBy(x => x.Date)
            .ToList();

        foreach (var evt in allEvents)
        {
            runningBalance += evt.Debit - evt.Credit;
            transactions.Add(new StatementTransactionDto(
                evt.Date,
                evt.Type,
                evt.Ref,
                evt.Debit,
                evt.Credit,
                runningBalance));
        }

        var totalInvoiced = openingInvoiced + periodInvoices.Sum(i => i.TotalAmount);
        var totalPaid = openingPaid + periodPayments.Sum(p => p.Amount);
        var closingBalance = totalInvoiced - totalPaid;

        var periodInvoiced = periodInvoices.Sum(i => i.TotalAmount);
        var periodPaid = periodPayments.Sum(p => p.Amount);

        return new ClientStatementDto(
            clientId,
            client.Name,
            from,
            to,
            openingPending,
            periodInvoiced,
            periodPaid,
            closingBalance,
            transactions);
    }

    public async Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var to = toDate ?? DateTime.UtcNow.Date;
        var from = fromDate ?? to.AddMonths(-1);

        var invoices = await invoiceRepository.GetAllWithPaymentsAsync(from, to, ct);

        var totalSales = invoices.Sum(i => i.TotalAmount);
        var totalCollected = invoices.Sum(i => i.PaidAmount);
        var totalOutstanding = invoices.Sum(i => i.PendingAmount);
        var paidCount = invoices.Count(i => i.PendingAmount <= 0);
        var pendingCount = invoices.Count(i => i.PendingAmount > 0);

        return new SalesSummaryDto(
            from,
            to,
            totalSales,
            totalCollected,
            totalOutstanding,
            invoices.Count,
            paidCount,
            pendingCount);
    }

    private static InvoiceSummaryItemDto ToSummaryItem(Invoice i, DateTime today) => new(
        i.Id,
        i.InvoiceNumber,
        i.Client?.Name ?? string.Empty,
        i.DueDate,
        i.TotalAmount,
        i.PaidAmount,
        i.PendingAmount,
        Math.Max(0, (today - i.DueDate.Date).Days));
}
