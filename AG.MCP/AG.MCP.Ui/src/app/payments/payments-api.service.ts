import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url.token';
import { PagedResult } from '../shared/models/paged-result.model';
import { toHttpParams } from '../shared/utils/http-query.util';
import { CreatePaymentRequest, PaymentDto } from './payments.models';

export interface PaymentListQuery {
  page?: number;
  pageSize?: number;
  search?: string | null;
  sortBy?: string | null;
  sortDescending?: boolean;
}

@Injectable({ providedIn: 'root' })
export class PaymentsApiService {
  private readonly baseUrl = `${inject(API_BASE_URL)}/api/payments`;

  constructor(private readonly http: HttpClient) {}

  getPaged(query: PaymentListQuery = {}): Observable<PagedResult<PaymentDto>> {
    const {
      page = 1,
      pageSize = 20,
      search,
      sortBy,
      sortDescending = false
    } = query;
    return this.http.get<PagedResult<PaymentDto>>(this.baseUrl, {
      params: toHttpParams({
        page,
        pageSize,
        search,
        sortBy,
        sortDescending
      })
    });
  }

  getById(id: string): Observable<PaymentDto> {
    return this.http.get<PaymentDto>(`${this.baseUrl}/${id}`);
  }

  create(request: CreatePaymentRequest): Observable<PaymentDto> {
    return this.http.post<PaymentDto>(this.baseUrl, request);
  }
}
