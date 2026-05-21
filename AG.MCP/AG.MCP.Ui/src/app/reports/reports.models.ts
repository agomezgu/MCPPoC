export interface InvoiceSummaryItemDto {
  invoiceId: string;
  invoiceNumber: string;
  clientName: string;
  dueDate: string;
  totalAmount: number;
  paidAmount: number;
  pendingAmount: number;
  daysOverdue: number;
}

export interface AccountsReceivableSummaryDto {
  totalReceivable: number;
  totalOverdue: number;
  totalPendingInvoices: number;
  totalOverdueInvoices: number;
  pendingInvoices: InvoiceSummaryItemDto[];
  overdueInvoices: InvoiceSummaryItemDto[];
}

export interface StatementTransactionDto {
  date: string;
  type: string;
  reference: string;
  debit: number;
  credit: number;
  balance: number;
}

export interface ClientStatementDto {
  clientId: string;
  clientName: string;
  fromDate: string;
  toDate: string;
  openingBalance: number;
  totalInvoiced: number;
  totalPaid: number;
  closingBalance: number;
  transactions: StatementTransactionDto[];
}

export interface SalesSummaryDto {
  fromDate: string;
  toDate: string;
  totalSales: number;
  totalCollected: number;
  totalOutstanding: number;
  invoiceCount: number;
  paidInvoiceCount: number;
  pendingInvoiceCount: number;
}
