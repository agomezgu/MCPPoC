namespace AG.MCP.Domain.Enums;

/// <summary>
/// Invoice payment status.
/// Status is calculated: pending → partial (if paidAmount > 0) → paid (if fully paid) → overdue (if past due and not paid).
/// </summary>
public enum InvoiceStatus
{
    Pending = 0,    // No payments yet
    Partial = 1,    // Some payment received
    Paid = 2,       // Fully paid
    Overdue = 3,    // Due date passed, pendingAmount > 0
    Cancelled = 4   // Invoice cancelled
}
