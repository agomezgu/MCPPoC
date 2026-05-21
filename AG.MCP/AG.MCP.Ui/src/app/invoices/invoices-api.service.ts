import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url.token';
import { PagedResult } from '../shared/models/paged-result.model';
import { toHttpParams } from '../shared/utils/http-query.util';
import {
  CreateInvoiceRequest,
  InvoiceDto,
  UpdateInvoiceRequest
} from './invoices.models';

export interface InvoiceListQuery {
  page?: number;
  pageSize?: number;
  search?: string | null;
  sortBy?: string | null;
  sortDescending?: boolean;
  clientId?: string | null;
}

@Injectable({ providedIn: 'root' })
export class InvoicesApiService {
  private readonly baseUrl = `${inject(API_BASE_URL)}/api/invoices`;

  constructor(private readonly http: HttpClient) {}

  getPaged(query: InvoiceListQuery = {}): Observable<PagedResult<InvoiceDto>> {
    const {
      page = 1,
      pageSize = 20,
      search,
      sortBy,
      sortDescending = false,
      clientId
    } = query;
    return this.http.get<PagedResult<InvoiceDto>>(this.baseUrl, {
      params: toHttpParams({
        page,
        pageSize,
        search,
        sortBy,
        sortDescending,
        clientId
      })
    });
  }

  getPending(
    query: Omit<InvoiceListQuery, 'clientId'> = {}
  ): Observable<PagedResult<InvoiceDto>> {
    const {
      page = 1,
      pageSize = 20,
      search,
      sortBy,
      sortDescending = false
    } = query;
    return this.http.get<PagedResult<InvoiceDto>>(`${this.baseUrl}/pending`, {
      params: toHttpParams({
        page,
        pageSize,
        search,
        sortBy,
        sortDescending
      })
    });
  }

  getOverdue(
    query: Omit<InvoiceListQuery, 'clientId'> = {}
  ): Observable<PagedResult<InvoiceDto>> {
    const {
      page = 1,
      pageSize = 20,
      search,
      sortBy,
      sortDescending = false
    } = query;
    return this.http.get<PagedResult<InvoiceDto>>(`${this.baseUrl}/overdue`, {
      params: toHttpParams({
        page,
        pageSize,
        search,
        sortBy,
        sortDescending
      })
    });
  }

  getById(id: string): Observable<InvoiceDto> {
    return this.http.get<InvoiceDto>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateInvoiceRequest): Observable<InvoiceDto> {
    return this.http.post<InvoiceDto>(this.baseUrl, request);
  }

  update(id: string, request: UpdateInvoiceRequest): Observable<InvoiceDto> {
    return this.http.put<InvoiceDto>(`${this.baseUrl}/${id}`, request);
  }
}
