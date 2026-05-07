export interface InvoiceItemDto {
  id: string;
  description: string;
  productCode: string | null;
  quantity: number;
  unit: string;
  unitPrice: number;
  lineTotal: number;
}

export interface InvoiceDto {
  id: string;
  invoiceNumber: string;
  clientId: string;
  clientName: string;
  issueDate: string;
  dueDate: string;
  totalAmount: number;
  paidAmount: number;
  pendingAmount: number;
  status: string;
  notes: string | null;
  createdAt: string;
  items: InvoiceItemDto[];
}

export interface CreateInvoiceItemRequest {
  description: string;
  productCode?: string | null;
  quantity: number;
  unit: string;
  unitPrice: number;
}

export interface CreateInvoiceRequest {
  clientId: string;
  issueDate: string;
  dueDate: string;
  notes?: string | null;
  items: CreateInvoiceItemRequest[];
}

export interface UpdateInvoiceRequest {
  issueDate: string;
  dueDate: string;
  notes?: string | null;
  items: CreateInvoiceItemRequest[];
}
