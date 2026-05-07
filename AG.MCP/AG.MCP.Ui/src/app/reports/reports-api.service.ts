import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api-base-url.token';
import { toHttpParams } from '../shared/utils/http-query.util';
import {
  AccountsReceivableSummaryDto,
  ClientStatementDto,
  SalesSummaryDto
} from './reports.models';

@Injectable({ providedIn: 'root' })
export class ReportsApiService {
  private readonly baseUrl = `${inject(API_BASE_URL)}/api/reports`;

  constructor(private readonly http: HttpClient) {}

  getAccountsReceivable(): Observable<AccountsReceivableSummaryDto> {
    return this.http.get<AccountsReceivableSummaryDto>(
      `${this.baseUrl}/accounts-receivable`
    );
  }

  getSalesSummary(fromDate?: string | null, toDate?: string | null): Observable<SalesSummaryDto> {
    return this.http.get<SalesSummaryDto>(`${this.baseUrl}/sales-summary`, {
      params: toHttpParams({ fromDate, toDate })
    });
  }

  getClientStatement(
    clientId: string,
    fromDate?: string | null,
    toDate?: string | null
  ): Observable<ClientStatementDto> {
    return this.http.get<ClientStatementDto>(
      `${this.baseUrl}/clients/${clientId}/statement`,
      {
        params: toHttpParams({ fromDate, toDate })
      }
    );
  }
}
