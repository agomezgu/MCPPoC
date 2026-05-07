import { PagedResult } from '../shared/models/paged-result.model';

export type { PagedResult };

export interface ClientDto {
  id: string;
  name: string;
  taxId: string;
  email: string | null;
  phone: string | null;
  address: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface CreateClientRequest {
  name: string;
  taxId: string;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
}

export interface UpdateClientRequest extends CreateClientRequest {
  isActive: boolean;
}

export type ClientFormValue = UpdateClientRequest;

export interface ClientSummaryDto {
  clientId: string;
  clientName: string;
  totalInvoices: number;
  pendingInvoices: number;
  overdueInvoices: number;
  totalBilled: number;
  totalPaid: number;
  totalPending: number;
  totalOverdue: number;
}
