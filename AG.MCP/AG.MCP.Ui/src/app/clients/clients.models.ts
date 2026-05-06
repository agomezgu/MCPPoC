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

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}
