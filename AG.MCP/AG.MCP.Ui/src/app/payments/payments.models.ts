export interface PaymentDto {
  id: string;
  invoiceId: string;
  invoiceNumber: string;
  amount: number;
  paymentDate: string;
  reference: string | null;
  notes: string | null;
  createdAt: string;
}

export interface CreatePaymentRequest {
  invoiceId: string;
  amount: number;
  paymentDate: string;
  reference?: string | null;
  notes?: string | null;
}
