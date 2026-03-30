using AG.MCP.Domain.Enums;

namespace AG.MCP.Application.Services;

/// <summary>
/// Calculates invoice status based on amounts and due date.
/// Rules: overdue when dueDate &lt; today and pendingAmount &gt; 0.
/// </summary>
public static class InvoiceStatusCalculator
{
    public static InvoiceStatus Calculate(decimal totalAmount, decimal paidAmount, DateTime dueDate, InvoiceStatus? currentStatus = null)
    {
        var pendingAmount = totalAmount - paidAmount;

        // Cancelled stays cancelled
        if (currentStatus == InvoiceStatus.Cancelled)
            return InvoiceStatus.Cancelled;

        if (pendingAmount <= 0)
            return InvoiceStatus.Paid;

        var today = DateTime.UtcNow.Date;
        var due = dueDate.Date;

        if (due < today)
            return InvoiceStatus.Overdue;

        return paidAmount > 0 ? InvoiceStatus.Partial : InvoiceStatus.Pending;
    }
}
