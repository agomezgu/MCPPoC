import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map, Observable } from 'rxjs';
import {
  ClientDto,
  ClientSummaryDto,
  CreateClientRequest,
  UpdateClientRequest
} from './clients.models';
import { API_BASE_URL } from '../api-base-url.token';
import { PagedResult } from '../shared/models/paged-result.model';
import { toHttpParams } from '../shared/utils/http-query.util';

export interface GetClientsQuery {
  page?: number;
  pageSize?: number;
  search?: string | null;
  sortBy?: string | null;
  sortDescending?: boolean;
}

@Injectable({ providedIn: 'root' })
export class ClientsApiService {
  private readonly clientsUrl = `${inject(API_BASE_URL)}/api/clients`;

  constructor(private readonly http: HttpClient) {}

  getClients(query: GetClientsQuery = {}): Observable<PagedResult<ClientDto>> {
    const {
      page = 1,
      pageSize = 20,
      search,
      sortBy,
      sortDescending = false
    } = query;
    return this.http.get<PagedResult<ClientDto>>(this.clientsUrl, {
      params: toHttpParams({
        page,
        pageSize,
        search,
        sortBy,
        sortDescending
      })
    });
  }

  getClientSummary(id: string): Observable<ClientSummaryDto> {
    return this.http.get<ClientSummaryDto>(`${this.clientsUrl}/${id}/summary`);
  }

  getClient(id: string): Observable<ClientDto> {
    return this.http
      .get<ClientDto>(`${this.clientsUrl}/${id}`)
      .pipe(map((client) => this.normalizeClient(client)));
  }

  createClient(payload: CreateClientRequest): Observable<ClientDto> {
    return this.http
      .post<ClientDto>(this.clientsUrl, this.normalizeRequest(payload))
      .pipe(map((client) => this.normalizeClient(client)));
  }

  updateClient(id: string, payload: UpdateClientRequest): Observable<ClientDto> {
    return this.http
      .put<ClientDto>(`${this.clientsUrl}/${id}`, this.normalizeRequest(payload))
      .pipe(map((client) => this.normalizeClient(client)));
  }

  private normalizeRequest<T extends CreateClientRequest>(payload: T): T {
    return {
      ...payload,
      name: payload.name.trim(),
      taxId: payload.taxId.trim(),
      email: payload.email?.trim() || null,
      phone: payload.phone?.trim() || null,
      address: payload.address?.trim() || null
    };
  }

  private normalizeClient(client: ClientDto): ClientDto {
    return {
      ...client,
      email: client.email ?? null,
      phone: client.phone ?? null,
      address: client.address ?? null
    };
  }
}
